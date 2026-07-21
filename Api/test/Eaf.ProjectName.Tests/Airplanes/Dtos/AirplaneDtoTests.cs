using Abp.Application.Services.Dto;
using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes.Dtos
{
    public class AirplaneDtoTests
    {
        [Fact]
        public void AirplaneDto_ShouldSetAllProperties()
        {
            // Arrange & Act
            var dto = new AirplaneDto
            {
                Id = 1,
                Number = "ABC-123",
                Model = "Boeing 737"
            };

            // Assert
            dto.Id.ShouldBe(1);
            dto.Number.ShouldBe("ABC-123");
            dto.Model.ShouldBe("Boeing 737");
        }

        [Fact]
        public void AirplaneDto_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var dto = new AirplaneDto();

            // Assert
            dto.Id.ShouldBe(0);
            dto.Number.ShouldBeNull();
            dto.Model.ShouldBeNull();
        }

        [Fact]
        public void AirplaneDto_Number_CanBeEmpty()
        {
            // Arrange & Act
            var dto = new AirplaneDto
            {
                Number = string.Empty
            };

            // Assert
            dto.Number.ShouldBeEmpty();
        }

        [Fact]
        public void AirplaneDto_Model_CanBeEmpty()
        {
            // Arrange & Act
            var dto = new AirplaneDto
            {
                Model = string.Empty
            };

            // Assert
            dto.Model.ShouldBeEmpty();
        }

        [Fact]
        public void AirplaneDto_ShouldInheritFromEntityDto()
        {
            // Arrange & Act
            var dto = new AirplaneDto();

            // Assert
            dto.ShouldBeAssignableTo<EntityDto>();
        }
    }
}
