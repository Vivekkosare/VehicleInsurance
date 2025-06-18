namespace InsuranceAPI.Features.FeatureManagement.DTOs;

public record FeatureToggleOutput(Guid Id, string Name, string Description, bool IsEnabled);
