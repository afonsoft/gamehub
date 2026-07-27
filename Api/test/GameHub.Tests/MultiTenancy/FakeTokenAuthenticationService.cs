using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eaf.Middleware.Web.Authentication;

namespace GameHub.Tests.MultiTenancy
{
    public class FakeTokenAuthenticationService : ITokenAuthenticationService
    {
        public Task<string> CreateAccessTokenAsync(IEnumerable<Claim> claims, System.TimeSpan expiration)
        {
            return Task.FromResult("fake-access-token");
        }

        public Task<Eaf.Middleware.Web.Models.TokenAuth.AuthenticateResultModel> AuthenticateAsync(Eaf.Middleware.Web.Models.TokenAuth.AuthenticateModel model)
        {
            throw new System.NotImplementedException();
        }
    }
}
