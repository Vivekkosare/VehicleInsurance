namespace InsuranceAPI.Features.FeatureManagement.DTOs;

public record FeatureTogglePatchDto(string? Description, bool? IsEnabled);
