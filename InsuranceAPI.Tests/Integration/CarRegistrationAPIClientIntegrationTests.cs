using InsuranceAPI.HttpClients;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Tests.Integration
{
    public class CarRegistrationAPIClientIntegrationTests : IClassFixture<TestVehicleRegistrationApiFactory>
    {
        private readonly HttpClient _client;
        private readonly CarRegistrationAPIClient _carRegistrationApiClient;
        private readonly string _personId_1 = "1234567890";
        private readonly string _personId_2 = "9999999999";

        public CarRegistrationAPIClientIntegrationTests(TestVehicleRegistrationApiFactory factory)
        {
            _client = factory.CreateClient();
            _carRegistrationApiClient = new CarRegistrationAPIClient(_client);
        }

        [Fact]
        public async Task GetCarRegistrationAsync_ReturnsCars_ForValidPersonId()
        {
            var testPersonId = _personId_1;
            var cars = await _carRegistrationApiClient.GetCarRegistrationAsync(testPersonId);

            Assert.NotNull(cars);
            Assert.All(cars, car => Assert.Equal(testPersonId, car.Owner.PersonalIdentificationNumber));
        }

        [Fact]
        public async Task GetCustomerByPersonalIdentificationNumberAsync_ReturnsCustomer_ForValidPersonId()
        {
            var testPersonId = _personId_1;
            var customer = await _carRegistrationApiClient.GetCustomerByPersonalIdentificationNumberAsync(testPersonId);

            Assert.NotNull(customer);
            Assert.Equal(testPersonId, customer.PersonalIdentificationNumber);
        }

        [Fact]
        public async Task GetCarRegistrationsByPersonIdsAsync_ReturnsCars_ForMultiplePersonIds()
        {
            var personIds = new PersonIdentifiersRequest(new[] { _personId_1, _personId_2 });
            var cars = await _carRegistrationApiClient.GetCarRegistrationsByPersonIdsAsync(personIds);

            Assert.NotNull(cars);
            Assert.All(cars, car => Assert.Contains(car.Owner.PersonalIdentificationNumber, personIds.PersonalIdentificationNumbers));
        }
    }
}
