using Abp.Auditing;
using Eaf.Middleware.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Eaf.ProjectName.Web.Controllers
{
    public class HomeController : MiddlewareControllerBase
    {
        [DisableAuditing]
        public IActionResult Index()
        {
            return Redirect($"/swagger");
        }
    }
}