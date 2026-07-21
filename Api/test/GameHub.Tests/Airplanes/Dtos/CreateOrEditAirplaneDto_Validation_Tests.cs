using GameHub.Airplanes;
using GameHub.Airplanes.Dtos;
using Shouldly;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GameHub.Tests.Airplanes.Dtos
{
    public class CreateOrEditAirplaneDto_Validation_Tests
    {
        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_TodosCamposPreenchidos_Entao_DeveSerValido()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Number = "VAL-001",
                Model = "Boeing 737"
            };

            // Quando (When)
            var results = ValidateModel(dto);

            // Então (Then)
            results.Count.ShouldBe(0);
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_NumberNulo_Entao_DeveSerInvalido()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Number = null,
                Model = "Boeing 737"
            };

            // Quando (When)
            var results = ValidateModel(dto);

            // Então (Then)
            results.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_ModelNulo_Entao_DeveSerInvalido()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Number = "VAL-002",
                Model = null
            };

            // Quando (When)
            var results = ValidateModel(dto);

            // Então (Then)
            results.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_ModelExcedeMaxLength_Entao_DeveSerInvalido()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Number = "VAL-003",
                Model = new string('A', Airplane.MaxModelLength + 1)
            };

            // Quando (When)
            var results = ValidateModel(dto);

            // Então (Then)
            results.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_ModelNoLimiteMaxLength_Entao_DeveSerValido()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Number = "VAL-004",
                Model = new string('A', Airplane.MaxModelLength)
            };

            // Quando (When)
            var results = ValidateModel(dto);

            // Então (Then)
            results.Count.ShouldBe(0);
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_IdNulo_Entao_IndicaCriacao()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Id = null,
                Number = "VAL-005",
                Model = "Embraer E190"
            };

            // Então (Then)
            dto.Id.ShouldBeNull();
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_IdPreenchido_Entao_IndicaEdicao()
        {
            // Dado (Given)
            var dto = new CreateOrEditAirplaneDto
            {
                Id = 42,
                Number = "VAL-006",
                Model = "Airbus A350"
            };

            // Então (Then)
            dto.Id.ShouldBe(42);
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
