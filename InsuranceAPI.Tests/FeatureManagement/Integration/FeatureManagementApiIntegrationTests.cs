using System.Net.Http.Json;
using System.Threading.Tasks;
using InsuranceAPI.Features.FeatureManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InsuranceAPI.Tests.FeatureManagement.Integration
{
    [Trait("TestCategory", "Integration")]
    public class FeatureManagementApiIntegrationTests : IClassFixture<InsuranceAPI.Tests.Integration.TestInsuranceApiFactory>
    {
        private readonly HttpClient _client;

        public FeatureManagementApiIntegrationTests(InsuranceAPI.Tests.Integration.TestInsuranceApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostAndGetFeatureToggle_ShouldSucceed()
        {
            // Arrange
            var input = new FeatureToggleInput("ApiIntegrationToggle", "Integration test toggle", true); // Always provide a non-null, non-empty Description

            // Act: POST to create
            var postResponse = await _client.PostAsJsonAsync("/api/v1/featuretoggles", input);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<FeatureToggleOutput>();
            Assert.NotNull(created); // Ensure created is not null before using its Id

            // Act: GET by id
            var getResponse = await _client.GetAsync($"/api/v1/featuretoggles/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<FeatureToggleOutput>();

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal("ApiIntegrationToggle", fetched.Name);
            Assert.True(fetched.IsEnabled);
        }
    }
}
