using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Exceptions;
using GameHub.Moderation.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    [AbpAuthorize]
    public class UserContentAppService : GameHubAppServiceBase, IUserContentAppService
    {
        private const int MaxTextLength = 2000;
        private const int MaxRequestsPerMinute = 10;
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromMinutes(5);

        private readonly IRepository<UserContent, Guid> _contentRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly ProfanityFilter _profanityFilter;
        private readonly ITypedCache<string, string> _rateLimitCache;
        private readonly ITypedCache<string, string> _idempotencyCache;

        public UserContentAppService(
            IRepository<UserContent, Guid> contentRepository,
            IRepository<Game, Guid> gameRepository,
            ICacheManager cacheManager)
        {
            _contentRepository = contentRepository;
            _gameRepository = gameRepository;
            _profanityFilter = new ProfanityFilter();
            _rateLimitCache = cacheManager
                .GetCache("GameHub.Moderation.UserContentRateLimit")
                .AsTyped<string, string>();
            _idempotencyCache = cacheManager
                .GetCache("GameHub.Moderation.UserContentIdempotency")
                .AsTyped<string, string>();
        }

        public async Task<UserContentDto> SubmitAsync(SubmitUserContentInput input)
        {
            await EnsureGameExistsAsync(input.GameId);
            await EnsureRateLimitAsync(input.GameId);

            var idempotencyKey = BuildIdempotencyKey(input.GameId, input.ClientRequestId);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingId = await _idempotencyCache.GetOrDefaultAsync(idempotencyKey);
                if (existingId != null)
                {
                    var existing = await _contentRepository.GetAsync(Guid.Parse(existingId));
                    return ObjectMapper.Map<UserContentDto>(existing);
                }
            }

            var normalizedText = NormalizeText(input.Text);
            var hasProfanity = _profanityFilter.ContainsProfanity(normalizedText);

            var content = new UserContent
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                ContentType = input.ContentType,
                Text = normalizedText,
                Rating = input.Rating,
                IsApproved = !hasProfanity,
                RequiresModeration = hasProfanity,
                ModerationReason = hasProfanity ? "Contains profanity" : null
            };

            await _contentRepository.InsertAsync(content);
            await CurrentUnitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                await _idempotencyCache.SetAsync(
                    idempotencyKey,
                    content.Id.ToString(),
                    absoluteExpireTime: DateTimeOffset.UtcNow.Add(IdempotencyWindow));
            }

            return ObjectMapper.Map<UserContentDto>(content);
        }

        private async Task EnsureGameExistsAsync(Guid gameId)
        {
            if (!await _gameRepository.GetAll().AnyAsync(g => g.Id == gameId && !g.IsDeleted))
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "O jogo informado não foi encontrado.",
                    retryable: false);
            }
        }

        private async Task EnsureRateLimitAsync(Guid gameId)
        {
            var key =
                $"gamehub:moderation:content:" +
                $"{AbpSession.TenantId?.ToString() ?? "host"}:" +
                $"{AbpSession.UserId}:" +
                $"{gameId:N}";

            var current = await _rateLimitCache.GetOrDefaultAsync(key);
            var count = int.TryParse(current, out var parsed) ? parsed : 0;

            if (count >= MaxRequestsPerMinute)
            {
                throw new GameHubException(
                    GameHubErrorCodes.RateLimited,
                    "Limite de envio de conteúdo excedido. Tente novamente mais tarde.",
                    retryable: true);
            }

            await _rateLimitCache.SetAsync(
                key,
                (count + 1).ToString(),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(RateLimitWindow));
        }

        private static string BuildIdempotencyKey(Guid gameId, string clientRequestId)
        {
            if (string.IsNullOrWhiteSpace(clientRequestId))
                return null;

            return $"gamehub:moderation:content:idempotency:{gameId:N}:{clientRequestId.Trim()}";
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "O texto do conteúdo é obrigatório.",
                    retryable: false);
            }

            var trimmed = text.Trim();
            if (trimmed.Length > MaxTextLength)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    $"O conteúdo excede o limite de {MaxTextLength} caracteres.",
                    retryable: false);
            }

            return trimmed;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<List<UserContentDto>> GetPendingAsync(int maxResultCount = 50)
        {
            var items = await _contentRepository.GetAll()
                .Where(c => !c.IsApproved || c.RequiresModeration)
                .OrderByDescending(c => c.CreationTime)
                .Take(maxResultCount)
                .ToListAsync();

            return ObjectMapper.Map<List<UserContentDto>>(items);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<UserContentDto> ModerateAsync(ModerateUserContentInput input)
        {
            await EnsureModerationIdempotencyAsync(input);

            var content = await _contentRepository.GetAll()
                .FirstOrDefaultAsync(c => c.Id == input.ContentId && c.TenantId == AbpSession.TenantId);

            if (content == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Conteúdo não encontrado para o tenant atual.",
                    retryable: false);
            }

            content.IsApproved = input.IsApproved;
            content.RequiresModeration = false;
            content.ModerationReason = input.Reason;

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<UserContentDto>(content);
        }

        private async Task EnsureModerationIdempotencyAsync(ModerateUserContentInput input)
        {
            if (string.IsNullOrWhiteSpace(input.ClientRequestId))
                return;

            var key = $"gamehub:moderation:decision:{input.ContentId:N}:{input.ClientRequestId.Trim()}";
            var existing = await _idempotencyCache.GetOrDefaultAsync(key);
            if (existing != null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "Decisão de moderação já registrada para este identificador.",
                    retryable: false);
            }

            await _idempotencyCache.SetAsync(
                key,
                "1",
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(IdempotencyWindow));
        }

        public async Task<List<UserContentDto>> GetByGameAsync(Guid gameId, bool onlyApproved = true)
        {
            var query = _contentRepository.GetAll().Where(c => c.GameId == gameId);
            if (onlyApproved)
            {
                query = query.Where(c => c.IsApproved && !c.RequiresModeration);
            }

            var items = await query.OrderByDescending(c => c.CreationTime).ToListAsync();
            return ObjectMapper.Map<List<UserContentDto>>(items);
        }
    }
}
