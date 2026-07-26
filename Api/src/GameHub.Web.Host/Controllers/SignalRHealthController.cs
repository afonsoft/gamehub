using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.Web.Controllers
{
    /// <summary>
    /// Exposes a lightweight liveness endpoint for SignalR hosting.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("health/signalr")]
    public class SignalRHealthController : ControllerBase
    {
        [HttpGet]
        public object Get()
        {
            return new { status = "healthy", signalR = true };
        }
    }
}
