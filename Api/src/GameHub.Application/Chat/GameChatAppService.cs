using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Data;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Timing;
using Eaf.Middleware.Chat;
using Eaf.Middleware.Authorization.Users;
using GameHub.Catalog;
using GameHub.Multiplayer;
using GameHub.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Chat
{
    /// <summary>
    /// Authorizes game chat against GameHub context and delegates persistence/delivery to EAF.
    /// </summary>
    [AbpAuthorize]
    public class GameChatAppService : GameHubAppServiceBase, IGameChatAppService
    {
        private static readonly TimeSpan DeduplicationWindow = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
        private const int MaxMessagesPerMinute = 30;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> _tenantRepository;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.UserTenantMembership, long> _membershipRepository;
        private readonly IChatMessageManager _chatMessageManager;
        private readonly ITypedCache<string, string> _deduplicationCache;
        private readonly ITypedCache<string, string> _rateLimitCache;

        public GameChatAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<MatchState, Guid> matchRepository,
            IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> tenantRepository,
            IRepository<Eaf.Middleware.MultiTenancy.UserTenantMembership, long> membershipRepository,
            IChatMessageManager chatMessageManager,
            ICacheManager cacheManager)
        {
            _gameRepository = gameRepository;
            _matchRepository = matchRepository;
            _tenantRepository = tenantRepository;
            _membershipRepository = membershipRepository;
            _chatMessageManager = chatMessageManager;
            _deduplicationCache = cacheManager
                .GetCache("GameHub.Chat.Deduplication")
                .AsTyped<string, string>();
            _rateLimitCache = cacheManager
                .GetCache("GameHub.Chat.RateLimit")
                .AsTyped<string, string>();
        }

        public async Task<GameChatMessageResult> SendAsync(SendGameChatMessageInput input)
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException("Authenticated chat is required.");
            }

            var text = NormalizeText(input.Text);
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Chat message cannot be empty.", nameof(input));
            }

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                await _gameRepository.GetAsync(input.GameId);
            }

            var playerTenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == GameHubConsts.PlayerTenantName)
                ?? throw new InvalidOperationException("Player tenant is not configured.");

            var deduplicationKey = await BuildDeduplicationKeyAsync(input, playerTenant.Id);
            if (await _deduplicationCache.GetOrDefaultAsync(deduplicationKey) != null)
            {
                return new GameChatMessageResult
                {
                    Accepted = true,
                    Duplicate = true,
                    ClientMessageId = input.ClientMessageId
                };
            }

            await EnsureRateLimitAsync(input.GameId, playerTenant.Id);

            using (UnitOfWorkManager.Current.SetTenantId(playerTenant.Id))
            using (UnitOfWorkManager.Current.EnableFilter(AbpDataFilters.MayHaveTenant))
            {
                var conversation = ParseConversation(input.ConversationId);
                var (sender, senderIdentifier) = await ResolveChatSenderAsync(playerTenant.Id);

                if (conversation.Kind == ConversationKind.Match)
                {
                    await SendToMatchAsync(conversation.MatchId.Value, input.GameId, senderIdentifier, sender, text);
                }
                else
                {
                    await SendToUserAsync(conversation, senderIdentifier, sender, text);
                }
            }

            await _deduplicationCache.SetAsync(
                deduplicationKey,
                Clock.Now.ToString("O"),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(DeduplicationWindow));

            return new GameChatMessageResult
            {
                Accepted = true,
                Duplicate = false,
                ClientMessageId = input.ClientMessageId
            };
        }

        private async Task<(User Sender, UserIdentifier SenderIdentifier)> ResolveChatSenderAsync(int playerTenantId)
        {
            User sender;
            if (AbpSession.TenantId == playerTenantId)
            {
                sender = await UserManager.GetUserByIdAsync(AbpSession.UserId.Value);
                return (sender, new UserIdentifier(playerTenantId, sender.Id));
            }

            var membership = await _membershipRepository.FirstOrDefaultAsync(m =>
                m.UserId == AbpSession.UserId.Value && m.TenantId == playerTenantId);

            if (membership == null)
            {
                throw new InvalidOperationException("User is not a member of the Player tenant.");
            }

            sender = await UserManager.GetUserByIdAsync(membership.TenantUserId);
            return (sender, new UserIdentifier(playerTenantId, sender.Id));
        }

        private async Task SendToMatchAsync(
            Guid matchId,
            Guid gameId,
            UserIdentifier senderIdentifier,
            User sender,
            string text)
        {
            var match = await _matchRepository.GetAll()
                .Include(item => item.Participants)
                .FirstOrDefaultAsync(item => item.Id == matchId && item.GameId == gameId);
            if (match == null || match.Status == MatchStatus.Ended)
            {
                throw new InvalidOperationException("Match is not available.");
            }

            var participant = match.Participants.FirstOrDefault(item =>
                item.IsActive && item.UserId == senderIdentifier.UserId);
            if (participant == null)
            {
                throw new InvalidOperationException("User is not an active match participant.");
            }

            foreach (var target in match.Participants
                         .Where(item => item.IsActive
                             && item.UserId.HasValue
                             && item.UserId.Value != senderIdentifier.UserId)
                         .Select(item => item.UserId.Value)
                         .Distinct())
            {
                await _chatMessageManager.SendMessageAsync(
                    senderIdentifier,
                    new UserIdentifier(AbpSession.TenantId.Value, target),
                    text,
                    null,
                    sender.UserName,
                    null);
            }
        }

        private async Task SendToUserAsync(
            Conversation conversation,
            UserIdentifier senderIdentifier,
            User sender,
            string text)
        {
            var chatTenantId = AbpSession.TenantId.Value;
            if (conversation.TenantId != chatTenantId || conversation.UserId == senderIdentifier.UserId)
            {
                throw new InvalidOperationException("Conversation target is not allowed.");
            }

            await _chatMessageManager.SendMessageAsync(
                senderIdentifier,
                new UserIdentifier(chatTenantId, conversation.UserId.Value),
                text,
                null,
                sender.UserName,
                null);
        }

        private async Task<string> BuildDeduplicationKeyAsync(SendGameChatMessageInput input, int playerTenantId)
        {
            if (string.IsNullOrWhiteSpace(input.ClientMessageId))
            {
                throw new ArgumentException(
                    "Client message id is required.",
                    nameof(SendGameChatMessageInput.ClientMessageId));
            }

            var chatTenantId = playerTenantId.ToString();
            return $"gamehub:chat:dedup:{chatTenantId}:{AbpSession.UserId}:{input.GameId:N}:{input.ConversationId}:{input.ClientMessageId}";
        }

        private async Task EnsureRateLimitAsync(Guid gameId, int playerTenantId)
        {
            var key = $"gamehub:chat:rate:{playerTenantId}:{AbpSession.UserId}:{gameId:N}";
            var current = await _rateLimitCache.GetOrDefaultAsync(key);
            var count = int.TryParse(current, out var parsed) ? parsed : 0;
            if (count >= MaxMessagesPerMinute)
            {
                throw new InvalidOperationException("Chat rate limit exceeded.");
            }

            await _rateLimitCache.SetAsync(
                key,
                (count + 1).ToString(),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(RateLimitWindow));
        }

        private static string NormalizeText(string text)
        {
            return new string(text.Normalize(NormalizationForm.FormC)
                .Where(character => !char.IsControl(character))
                .Take(500)
                .ToArray());
        }

        private static Conversation ParseConversation(string conversationId)
        {
            var parts = conversationId.Split(':');
            if (parts.Length == 2 && parts[0] == "match" && Guid.TryParse(parts[1], out var matchId))
            {
                return new Conversation(ConversationKind.Match, matchId, null, null);
            }

            if (parts.Length == 3
                && parts[0] == "user"
                && int.TryParse(parts[1], out var tenantId)
                && long.TryParse(parts[2], out var userId)
                && userId > 0)
            {
                return new Conversation(ConversationKind.User, null, userId, tenantId);
            }

            throw new ArgumentException("Invalid conversation id.", nameof(conversationId));
        }

        private enum ConversationKind
        {
            Match,
            User
        }

        private sealed class Conversation
        {
            public Conversation(ConversationKind kind, Guid? matchId, long? userId, int? tenantId)
            {
                Kind = kind;
                MatchId = matchId;
                UserId = userId;
                TenantId = tenantId;
            }

            public ConversationKind Kind { get; }
            public Guid? MatchId { get; }
            public long? UserId { get; }
            public int? TenantId { get; }
        }
    }
}
