using GameHub.Airplanes;
using GameHub.EntityHistory;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace GameHub.Tests.Core.Application.EntityHistory
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
        public void Dado_EntityHistoryHelper_Quando_VerificarGameHubTrackedTypes_Entao_DeveConterTiposEsperados()
        {
            var trackedTypes = EntityHistoryHelper.GameHubTrackedTypes;

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
