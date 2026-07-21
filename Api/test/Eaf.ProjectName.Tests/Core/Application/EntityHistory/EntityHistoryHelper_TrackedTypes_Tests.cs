using Eaf.ProjectName.Airplanes;
using Eaf.ProjectName.EntityHistory;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.EntityHistory
{
    public class EntityHistoryHelper_TrackedTypes_Tests
    {
        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTrackedTypes_Entao_DeveConterTipos()
        {
            var trackedTypes = EntityHistoryHelper.TrackedTypes;

            trackedTypes.ShouldNotBeNull();
            trackedTypes.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTrackedTypes_Entao_DeveConterAirplane()
        {
            var trackedTypes = EntityHistoryHelper.TrackedTypes;

            trackedTypes.ShouldContain(typeof(Airplane));
        }

        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarProjectNameTrackedTypes_Entao_DeveConterTiposEsperados()
        {
            var trackedTypes = EntityHistoryHelper.ProjectNameTrackedTypes;

            trackedTypes.ShouldNotBeNull();
            trackedTypes.Length.ShouldBe(6);
        }

        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTrackedTypes_Entao_NaoDeveConterDuplicatas()
        {
            var trackedTypes = EntityHistoryHelper.TrackedTypes;
            var distinct = trackedTypes.Select(t => t.FullName).Distinct().Count();

            distinct.ShouldBe(trackedTypes.Length);
        }

        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTrackedTypes_Entao_DeveConterRole()
        {
            var trackedTypes = EntityHistoryHelper.TrackedTypes;

            trackedTypes.ShouldContain(t => t.Name == "Role");
        }

        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTrackedTypes_Entao_DeveConterUser()
        {
            var trackedTypes = EntityHistoryHelper.TrackedTypes;

            trackedTypes.ShouldContain(t => t.Name == "User");
        }

        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTrackedTypes_Entao_DeveConterTenant()
        {
            var trackedTypes = EntityHistoryHelper.TrackedTypes;

            trackedTypes.ShouldContain(t => t.Name == "Tenant");
        }
    }
}
