using Abp.Dependency;
using System.Threading.Tasks;

namespace GameHub.Airplanes.Jobs
{
    public interface IAirplaneJob : ITransientDependency
    {
        Task StartProcess();
    }
}