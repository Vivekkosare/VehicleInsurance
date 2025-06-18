using InsuranceAPI.Features.FeatureManagement.Entities;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Repositories;

public interface IFeatureManagementRepository
{
    Task<Result<IEnumerable<FeatureToggle>>> GetAllFeatureTogglesAsync();
    Task<Result<FeatureToggle?>> GetFeatureToggleByIdAsync(Guid id);
    Task<Result<bool>> IsFeatureToggleEnabledAsync(string name);
    Task<Result<FeatureToggle>> AddFeatureToggleAsync(FeatureToggle featureToggle);
    Task<Result<FeatureToggle>> PatchFeatureToggleAsync(Guid id, FeatureToggle featureToggle);
    Task<Result<bool>> DeleteFeatureToggleAsync(Guid id);
}
