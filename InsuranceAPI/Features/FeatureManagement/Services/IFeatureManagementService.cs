using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Entities;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Services;

public interface IFeatureManagementService
{
    Task<Result<IEnumerable<FeatureToggleOutput>>> GetAllFeatureTogglesAsync();
    Task<Result<FeatureToggleOutput>> GetFeatureToggleByIdAsync(Guid id);
    Task<Result<FeatureToggleNameOutput>> GetFeatureTogglesByNamesAsync(FeatureToggleNameInput input);
    Task<Result<FeatureToggleOutput>> AddFeatureToggleAsync(FeatureToggleInput featureToggleInput);
    Task<Result<FeatureToggleOutput>> PatchFeatureToggleAsync(Guid id, FeatureToggleInput featureToggleInput);
    Task<Result<bool>> DeleteFeatureToggleAsync(Guid id);
}
