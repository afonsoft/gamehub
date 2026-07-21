using Abp.Application.Services.Dto;
using Eaf.ProjectName.Airplanes;
using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes.Dtos
{
    public class CreateOrEditAirplaneDtoTests
    {
        [Fact]
        public void CreateOrEditAirplaneDto_ShouldSetAllProperties()
        {
            // Arrange & Act
            var dto = new CreateOrEditAirplaneDto
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
        public void CreateOrEditAirplaneDto_Id_CanBeNull()
        {
            // Arrange & Act
            var dto = new CreateOrEditAirplaneDto
            {
                Id = null
            };

            // Assert
            dto.Id.ShouldBeNull();
        }

        [Fact]
        public void CreateOrEditAirplaneDto_Number_ShouldHaveRequiredAttribute()
        {
            // Arrange
            var property = typeof(CreateOrEditAirplaneDto).GetProperty("Number");

            // Act
            var attributes = property.GetCustomAttributes(typeof(RequiredAttribute), false);

            // Assert
            attributes.ShouldNotBeEmpty();
        }

        [Fact]
        public void CreateOrEditAirplaneDto_Model_ShouldHaveRequiredAttribute()
        {
            // Arrange
            var property = typeof(CreateOrEditAirplaneDto).GetProperty("Model");

            // Act
            var attributes = property.GetCustomAttributes(typeof(RequiredAttribute), false);

            // Assert
            attributes.ShouldNotBeEmpty();
        }

        [Fact]
        public void CreateOrEditAirplaneDto_Model_ShouldHaveStringLengthAttribute()
        {
            // Arrange
            var property = typeof(CreateOrEditAirplaneDto).GetProperty("Model");

            // Act
            var attributes = property.GetCustomAttributes(typeof(StringLengthAttribute), false);

            // Assert
            attributes.ShouldNotBeEmpty();
        }

        [Fact]
        public void CreateOrEditAirplaneDto_ShouldInheritFromEntityDtoOfNullableInt()
        {
            // Arrange & Act
            var dto = new CreateOrEditAirplaneDto();

            // Assert
            dto.ShouldBeAssignableTo<EntityDto<int?>>();
        }

        [Fact]
        public void CreateOrEditAirplaneDto_ShouldHaveAutoMapAttribute()
        {
            // Arrange
            var type = typeof(CreateOrEditAirplaneDto);

            // Act
            var attributes = type.GetCustomAttributes(typeof(Abp.AutoMapper.AutoMapAttribute), false);

            // Assert
            attributes.ShouldNotBeEmpty();
        }

        [Fact]
        public void CreateOrEditAirplaneDto_StringLength_MaxModelLength_ShouldBeGreaterThanZero()
        {
            // Assert
            Airplane.MaxModelLength.ShouldBeGreaterThan(0);
        }
    }
}
