using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Eaf.Middleware.Chat;
using Eaf.Middleware.Authorization.Users;
using GameHub.Catalog;
using GameHub.Multiplayer;
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
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IChatMessageManager _chatMessageManager;
        private readonly ITypedCache<string, string> _deduplicationCache;

        public GameChatAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<MatchState, Guid> matchRepository,
            IChatMessageManager chatMessageManager,
            ICacheManager cacheManager)
        {
            _gameRepository = gameRepository;
            _matchRepository = matchRepository;
            _chatMessageManager = chatMessageManager;
            _deduplicationCache = cacheManager
                .GetCache("GameHub.Chat.Deduplication")
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

            await _gameRepository.GetAsync(input.GameId);
            var deduplicationKey = BuildDeduplicationKey(input);
            if (await _deduplicationCache.GetOrDefaultAsync(deduplicationKey) != null)
            {
                return new GameChatMessageResult
                {
                    Accepted = true,
                    Duplicate = true,
                    ClientMessageId = input.ClientMessageId
                };
            }

            var conversation = ParseConversation(input.ConversationId);
            var sender = await UserManager.GetUserByIdAsync(AbpSession.UserId.Value);
            var senderIdentifier = new UserIdentifier(AbpSession.TenantId, sender.Id);

            if (conversation.Kind == ConversationKind.Match)
            {
                await SendToMatchAsync(conversation.MatchId.Value, input.GameId, senderIdentifier, sender, text);
            }
            else
            {
                await SendToUserAsync(conversation, senderIdentifier, sender, text);
            }

            await _deduplicationCache.SetAsync(
                deduplicationKey,
                DateTime.UtcNow.ToString("O"),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(DeduplicationWindow));

            return new GameChatMessageResult
            {
                Accepted = true,
                Duplicate = false,
                ClientMessageId = input.ClientMessageId
            };
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
                    new UserIdentifier(match.TenantId, target),
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
            if (conversation.TenantId != AbpSession.TenantId || conversation.UserId == senderIdentifier.UserId)
            {
                throw new InvalidOperationException("Conversation target is not allowed.");
            }

            await _chatMessageManager.SendMessageAsync(
                senderIdentifier,
                new UserIdentifier(conversation.TenantId, conversation.UserId.Value),
                text,
                null,
                sender.UserName,
                null);
        }

        private string BuildDeduplicationKey(SendGameChatMessageInput input)
        {
            if (string.IsNullOrWhiteSpace(input.ClientMessageId))
            {
                throw new ArgumentException(
                    "Client message id is required.",
                    nameof(SendGameChatMessageInput.ClientMessageId));
            }

            return $"gamehub:chat:dedup:{AbpSession.TenantId?.ToString() ?? "host"}:{AbpSession.UserId}:{input.GameId:N}:{input.ConversationId}:{input.ClientMessageId}";
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
