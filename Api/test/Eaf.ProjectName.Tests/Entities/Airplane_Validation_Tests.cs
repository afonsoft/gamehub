using Eaf.ProjectName.Airplanes;
using Shouldly;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Eaf.ProjectName.Tests.Entities
{
    public class Airplane_Validation_Tests
    {
        [Fact]
        public void Dado_Airplane_Quando_MaxModelLength_Entao_DeveSerIgualA256()
        {
            // Então (Then)
            Airplane.MaxModelLength.ShouldBe(256);
        }

        [Fact]
        public void Dado_Airplane_Quando_NumberNull_Entao_DeveSerInvalido()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = null,
                Model = "Boeing 737"
            };

            // Quando (When)
            var results = ValidateModel(airplane);

            // Então (Then)
            results.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_Airplane_Quando_ModelNull_Entao_DeveSerInvalido()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = "TEST-001",
                Model = null
            };

            // Quando (When)
            var results = ValidateModel(airplane);

            // Então (Then)
            results.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_Airplane_Quando_ModelExcedeMaxLength_Entao_DeveSerInvalido()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = "TEST-002",
                Model = new string('X', Airplane.MaxModelLength + 1)
            };

            // Quando (When)
            var results = ValidateModel(airplane);

            // Então (Then)
            results.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_Airplane_Quando_ModelNoLimiteMaxLength_Entao_DeveSerValido()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = "TEST-003",
                Model = new string('X', Airplane.MaxModelLength)
            };

            // Quando (When)
            var results = ValidateModel(airplane);

            // Então (Then)
            results.Count.ShouldBe(0);
        }

        [Fact]
        public void Dado_Airplane_Quando_TenantIdNull_Entao_DeveSerValido()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = "TEST-004",
                Model = "Cessna 172",
                TenantId = null
            };

            // Então (Then)
            airplane.TenantId.ShouldBeNull();
        }

        [Fact]
        public void Dado_Airplane_Quando_IMayHaveTenant_Entao_DeveImplementarInterface()
        {
            // Dado (Given)
            var airplane = new Airplane();

            // Então (Then)
            airplane.ShouldBeAssignableTo<Abp.Domain.Entities.IMayHaveTenant>();
        }

        [Fact]
        public void Dado_Airplane_Quando_FullAuditedEntity_Entao_DeveHerdarDeFullAuditedEntity()
        {
            // Dado (Given)
            var airplane = new Airplane();

            // Então (Then)
            airplane.ShouldBeAssignableTo<Abp.Domain.Entities.Auditing.FullAuditedEntity>();
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
