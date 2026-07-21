using Abp.Domain.Services;
using Hangfire.Server;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Airplanes
{
    public interface IAirplaneManager : IDomainService
    {
        IQueryable<Airplane> Airplanes { get; }

        Task<Airplane> CreateAsync(Airplane airplane);

        Task<Airplane> UpdateAsync(Airplane airplane);

        Task DeleteAsync(int id);

        Task<Airplane> GetByIdAsync(int id);

        [DisplayName("Job DateUpdate")]
        Task DateUpdate(PerformContext context);
    }
}