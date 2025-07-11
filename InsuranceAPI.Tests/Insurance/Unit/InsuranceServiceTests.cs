using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Services;
using InsuranceAPI.Features.Insurance.DTOs;
using InsuranceAPI.Features.Insurance.Entities;
using InsuranceAPI.Features.Insurance.Pricing;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.Features.Insurance.Services;
using InsuranceAPI.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Tests.Insurance
{
    public class InsuranceServiceTests
    {
        private readonly Mock<IInsuranceRepository> _repoMock;
        private readonly Mock<ICarRegistrationAPIClient> _apiClientMock;
        private readonly Mock<ILogger<InsuranceService>> _loggerMock;
        private readonly Mock<IFeatureManagementService> _featureManagementServiceMock;
        private readonly Mock<IPriceCalculatorFactory> _priceCalculatorFactoryMock;
        private readonly InsuranceService _service;

        public InsuranceServiceTests()
        {
            _repoMock = new Mock<IInsuranceRepository>();
            _apiClientMock = new Mock<ICarRegistrationAPIClient>();
            _loggerMock = new Mock<ILogger<InsuranceService>>();
            _featureManagementServiceMock = new Mock<IFeatureManagementService>();
            _priceCalculatorFactoryMock = new Mock<IPriceCalculatorFactory>();
            _service = new InsuranceService(_repoMock.Object, _apiClientMock.Object, _loggerMock.Object, _featureManagementServiceMock.Object, _priceCalculatorFactoryMock.Object);
        }

        private static OwnerDto CreateOwnerDto(string pin)
        {
            return new OwnerDto(Guid.NewGuid(), "Test User", pin, "test@example.com", "1234567890");
        }

        private static CustomerOutput CreateCustomerOutput(string pin)
        {
            return new CustomerOutput(Guid.NewGuid(), "Test User", pin, "test@example.com", "1234567890");
        }
        private static CarDto CreateCarDto(string registrationNumber, string pin)
        {
            var owner = CreateOwnerDto(pin);
            return new CarDto(
                Id: Guid.NewGuid().ToString(),
                Name: "Test Car",
                RegistrationNumber: registrationNumber,
                Make: "TestMake",
                Model: "TestModel",
                Year: 2020,
                Color: "Red",
                RegistrationDate: DateTime.UtcNow,
                Owner: owner
            );
        }

        [Fact]
        public async Task AddInsuranceAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange the test data
            var input = new InsuranceInput(Guid.NewGuid(), "123", "Car", DateTime.UtcNow, DateTime.UtcNow.AddYears(1));
            var product = new InsuranceProduct { Id = input.InsuranceProductId, Code = "CAR" };
            var customer = CreateCustomerOutput(input.PersonalIdentificationNumber);
            var carDto = CreateCarDto(input.InsuredItemIdentity, input.PersonalIdentificationNumber);

            // Setup mocks
            _repoMock.Setup(r => r.GetInsuranceProductByIdAsync(input.InsuranceProductId)).ReturnsAsync(Result<InsuranceProduct>.Success(product));
            _apiClientMock.Setup(a => a.GetCustomerByPersonalIdentificationNumberAsync(input.PersonalIdentificationNumber)).ReturnsAsync(customer);
            _apiClientMock.Setup(a => a.GetCarRegistrationAsync(input.PersonalIdentificationNumber)).ReturnsAsync(new List<CarDto> { carDto });
            _repoMock.Setup(r => r.AddInsuranceAsync(It.IsAny<Features.Insurance.Entities.Insurance>())).ReturnsAsync(Result<Features.Insurance.Entities.Insurance>.Success(
                new Features.Insurance.Entities.Insurance { Id = Guid.NewGuid(),
                    InsuranceProduct = product,
                    PersonalIdentificationNumber = input.PersonalIdentificationNumber,
                    InsuredItemIdentity = input.InsuredItemIdentity,
                    Price = 100.0m, // Assuming a fixed price for simplicity
                    DiscountApplied = false,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate }));
            _featureManagementServiceMock.Setup(f => f.GetFeatureTogglesByNamesAsync(It.IsAny<FeatureToggleNameInput>()))
                .ReturnsAsync(Result<IEnumerable<FeatureToggleOutput>>.Success(new[]
                {
                    new FeatureToggleOutput(Guid.NewGuid(), "ApplyDiscounts", string.Empty, false)
                }));
            _featureManagementServiceMock.Setup(f => f.GetFeatureToggleByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<FeatureToggleOutput>.Success(new FeatureToggleOutput(Guid.NewGuid(), "ShowCarDetails", string.Empty, false)));

            var priceCalculatorMock = new Mock<IPriceCalculator>();
            priceCalculatorMock.Setup(p => p.CalculatePrice(It.IsAny<InsuranceProduct>())).Returns(100.0m);
            _priceCalculatorFactoryMock.Setup(f => f.GetPriceCalculator(It.IsAny<bool>())).Returns(priceCalculatorMock.Object);

            // call the add insurance method
            var result = await _service.AddInsuranceAsync(input);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(input.PersonalIdentificationNumber, result.Value.PersonalIdentificationNumber);
        }

        [Fact]
        public async Task GetInsurancesByPersonalIdentificationNumberAsync_ShouldReturnInsurances_WhenCustomerExists()
        {
            // Arrange the test data
            var pin = "123";
            var product = new InsuranceProduct { Id = Guid.NewGuid(), Code = "CAR" };
            var insurances = new List<Features.Insurance.Entities.Insurance> {
                new Features.Insurance.Entities.Insurance { Id = Guid.NewGuid(),
                InsuranceProduct = product,
                PersonalIdentificationNumber = pin,
                InsuredItemIdentity = "Car",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1)
                } };
            var customer = CreateCustomerOutput(pin);
            var carDto = CreateCarDto("Car", pin);

            // Setup mocks
            _apiClientMock.Setup(a => a.GetCustomerByPersonalIdentificationNumberAsync(pin)).ReturnsAsync(customer);
            _repoMock.Setup(r => r.GetInsurancesByPersonalIdentificationNumberAsync(pin)).ReturnsAsync(Result<IEnumerable<Features.Insurance.Entities.Insurance>>.Success(insurances));
            _apiClientMock.Setup(a => a.GetCarRegistrationAsync(pin)).ReturnsAsync(new List<CarDto> { carDto });
            _featureManagementServiceMock.Setup(f => f.GetFeatureToggleByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<FeatureToggleOutput>.Success(new FeatureToggleOutput(Guid.NewGuid(), "ShowCarDetails", string.Empty, false)));

            // Call the method to get insurances by personal identification number
            var result = await _service.GetInsurancesByPersonalIdentificationNumberAsync(pin);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal(pin, result.Value.First().PersonalIdentificationNumber);
        }

        [Fact]
        public async Task GetAllInsurancesAsync_ShouldReturnAllInsurances()
        {
            // Arrange the test data
            var product = new InsuranceProduct { Id = Guid.NewGuid(), Code = "CAR" };
            var insurances = new List<Features.Insurance.Entities.Insurance> {
                new Features.Insurance.Entities.Insurance { Id = Guid.NewGuid(),
                InsuranceProduct = product,
                PersonalIdentificationNumber = "123",
                InsuredItemIdentity = "Car",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1)
                } };
            var carDto = CreateCarDto("Car", "123");
            _repoMock.Setup(r => r.GetAllInsurancesAsync()).ReturnsAsync(Result<IEnumerable<Features.Insurance.Entities.Insurance>>.Success(insurances));
            _apiClientMock.Setup(a => a.GetCarRegistrationsByPersonIdsAsync(It.IsAny<PersonIdentifiersRequest>())).ReturnsAsync(new List<CarDto> { carDto });
            _featureManagementServiceMock.Setup(f => f.GetFeatureToggleByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<FeatureToggleOutput>.Success(new FeatureToggleOutput(Guid.NewGuid(), "ShowCarDetails", string.Empty, false)));

            // Call the method to get all insurances
            var result = await _service.GetAllInsurancesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal("123", result.Value.First().PersonalIdentificationNumber);
        }
    }
}
