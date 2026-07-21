using Abp.Dependency;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Airplanes.Jobs
{
    public interface IAirplaneJob : ITransientDependency
    {
        Task StartProcess();
    }
}