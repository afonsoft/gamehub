using System.Threading.Tasks;
using Abp.Application.Services;

namespace GameHub.Chat
{
    /// <summary>
    /// Provides contextual chat operations for games.
    /// </summary>
    public interface IGameChatAppService : IApplicationService
    {
        /// <summary>
        /// Sends an authenticated message to a validated conversation.
        /// </summary>
        Task<GameChatMessageResult> SendAsync(SendGameChatMessageInput input);
    }
}
