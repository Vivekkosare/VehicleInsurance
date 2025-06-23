namespace InsuranceAPI.Features.FeatureManagement.DTOs
{
    public record FeatureToggleInput(string Name, string Description, bool? IsEnabled);
}