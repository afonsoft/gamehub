using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Authorization.Dto;

namespace GameHub.Authorization
{
    /// <summary>
    /// Public registration application service.
    /// </summary>
    public interface IRegistrationAppService : IApplicationService
    {
        /// <summary>
        /// Registers a new local user.
        /// </summary>
        Task<RegisterOutput> RegisterAsync(RegisterInput input);
    }
}
