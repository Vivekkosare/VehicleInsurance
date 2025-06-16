using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.Features.Insurance.Entities;
using InsuranceAPI.Data;
using VehicleInsurance.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Linq;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Tests
{
    public class InsuranceRepositoryTests
    {
        private readonly ILogger<InsuranceRepository> _logger;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly DbContextOptions<InsuranceDbContext> _dbContextOptions;

        public InsuranceRepositoryTests()
        {
            _logger = Mock.Of<ILogger<InsuranceRepository>>();
            _cacheMock = new Mock<ICacheService>();
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["TimeoutInMinutes"]).Returns("10");
            _dbContextOptions = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddInsuranceAsync_ShouldReturnSuccess_WhenInsuranceIsNew()
        {
            using var dbContext = new InsuranceDbContext(_dbContextOptions);
            var repository = new InsuranceRepository(dbContext, _logger, _cacheMock.Object, _configMock.Object);

            var insurance = new Insurance
            {
                Id = Guid.NewGuid(),
                PersonalIdentificationNumber = "123",
                InsuredItem = "Car",
                InsuranceProductId = Guid.NewGuid()
            };

            _cacheMock
            .Setup(c => c.GetAsync<Insurance>(It.IsAny<string>()))
            .ReturnsAsync(Result<Insurance?>.Failure("Not found"));

            var result = await repository.AddInsuranceAsync(insurance);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetInsurancesByPersonalIdentificationNumberAsync_ShouldReturnInsurances_WhenFoundInCache()
        {
            using var dbContext = new InsuranceDbContext(_dbContextOptions);
            var repository = new InsuranceRepository(dbContext, _logger, _cacheMock.Object, _configMock.Object);
            var pin = "123";

            var insurances = new List<Insurance>
            {
                new Insurance { Id = Guid.NewGuid(),
                PersonalIdentificationNumber = pin,
                InsuredItem = "Car",
                InsuranceProductId = Guid.NewGuid()
            } };

            _cacheMock
            .Setup(c => c.GetAsync<IEnumerable<Insurance>>(It.IsAny<string>()))
            .ReturnsAsync(Result<IEnumerable<Insurance>?>.Success(insurances));

            var result = await repository.GetInsurancesByPersonalIdentificationNumberAsync(pin);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal(pin, result.Value.First().PersonalIdentificationNumber);
        }
    }
}
