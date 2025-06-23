using InsuranceAPI.Features.FeatureManagement.DTOs;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Services;

public interface IFeatureManagementService
{
    Task<Result<IEnumerable<FeatureToggleOutput>>> GetAllFeatureTogglesAsync();
    Task<Result<FeatureToggleOutput>> GetFeatureToggleByIdAsync(Guid id);
    Task<Result<FeatureToggleOutput>> GetFeatureToggleByNameAsync(string name);
    Task<Result<IEnumerable<FeatureToggleOutput>>> GetFeatureTogglesByNamesAsync(FeatureToggleNameInput input);
    Task<Result<FeatureToggleOutput>> AddFeatureToggleAsync(FeatureToggleInput featureToggleInput);
    Task<Result<FeatureToggleOutput>> PatchFeatureToggleAsync(string name, FeatureTogglePatchDto featureTogglePatchDto);
    Task<Result<bool>> DeleteFeatureToggleAsync(Guid id);
}
