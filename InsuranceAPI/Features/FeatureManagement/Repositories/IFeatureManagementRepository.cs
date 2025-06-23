using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Entities;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Repositories;

public interface IFeatureManagementRepository
{
    Task<Result<IEnumerable<FeatureToggle>>> GetAllFeatureTogglesAsync();
    Task<Result<FeatureToggle?>> GetFeatureToggleByIdAsync(Guid id);
    Task<Result<FeatureToggle>> GetFeatureToggleByNameAsync(string name);
    Task<Result<IEnumerable<FeatureToggle>>> GetFeatureTogglesByNamesAsync(List<string> names);
    Task<Result<FeatureToggle>> AddFeatureToggleAsync(FeatureToggle featureToggle);
    Task<Result<FeatureToggle>> PatchFeatureToggleAsync(string name, FeatureTogglePatchDto featureTogglePatchDto);
    Task<Result<bool>> DeleteFeatureToggleAsync(Guid id);
}
