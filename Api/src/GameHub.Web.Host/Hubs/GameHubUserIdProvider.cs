using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace GameHub.Web.Hubs
{
    /// <summary>
    /// Maps SignalR user identifiers to the ABP-compatible subject claim.
    /// </summary>
    public class GameHubUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("sub")?.Value
                   ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
