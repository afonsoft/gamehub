using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Developer.Dto;

namespace GameHub.Builds
{
    public interface IGameBuildPackageValidator
    {
        Task<ValidationSummaryDto> ValidateAsync(Stream packageStream, CancellationToken cancellationToken = default);
    }
}
