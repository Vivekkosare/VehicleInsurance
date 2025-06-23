using System.Net.Http.Json;
using System.Threading.Tasks;
using InsuranceAPI.Features.FeatureManagement.DTOs;
using Xunit;

namespace InsuranceAPI.Tests.FeatureManagement.Integration
{
    [Trait("TestCategory", "Integration")]
    public class FeatureManagementApiPatchIntegrationTests : IClassFixture<Insurance.Integration.TestInsuranceApiFactory>
    {
        private readonly HttpClient _client;

        public FeatureManagementApiPatchIntegrationTests(Insurance.Integration.TestInsuranceApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PatchFeatureToggle_ShouldUpdateDescriptionAndIsEnabled()
        {
            // Arrange: create a toggle first
            var input = new FeatureToggleInput("PatchIntegrationToggle", "Initial description", false);
            var postResponse = await _client.PostAsJsonAsync("/api/v1/featuretoggles", input);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<FeatureToggleOutput>();
            Assert.NotNull(created);

            // Act: PATCH to update description and isEnabled
            var patchDto = new FeatureTogglePatchDto("Updated description", true);
            var patchContent = JsonContent.Create(patchDto);
            var patchContentString = await patchContent.ReadAsStringAsync();
            // Log the outgoing PATCH request body for debugging
            System.Console.WriteLine($"PATCH BODY: {patchContentString}");
            var patchResponse = await _client.PatchAsJsonAsync($"/api/v1/featuretoggles/{created.Name}", patchDto);
            patchResponse.EnsureSuccessStatusCode();
            var patched = await patchResponse.Content.ReadFromJsonAsync<FeatureToggleOutput>();

            // Assert
            Assert.NotNull(patched);
            Assert.Equal("Updated description", patched.Description);
            Assert.True(patched.IsEnabled);
        }
    }
}
