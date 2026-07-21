using Eaf.ProjectName.Airplanes.Jobs;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Jobs
{
    public class AirplaneJob_Tests : ProjectNameTestBase
    {
        private readonly IAirplaneJob _airplaneJob;

        public AirplaneJob_Tests()
        {
            _airplaneJob = LocalIocManager.Resolve<IAirplaneJob>();
        }

        [Fact]
        public async Task Dado_JobInicializado_Quando_IniciarProcesso_Entao_DeveExecutarComSucesso()
        {
            // Dado (Given)
            
            // Quando (When)
            await Should.NotThrowAsync(async () => await _airplaneJob.StartProcess());
            
            // Então (Then)
            // Job deve executar sem lançar exceção
        }
    }
}
