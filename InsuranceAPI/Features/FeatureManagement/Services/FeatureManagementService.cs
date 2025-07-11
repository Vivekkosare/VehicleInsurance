using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Extensions;
using InsuranceAPI.Features.FeatureManagement.Repositories;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Services
{
    public class FeatureManagementService(IFeatureManagementRepository _repo,
    ILogger<FeatureManagementService> _logger) : IFeatureManagementService
    {
        public async Task<Result<FeatureToggleOutput>> AddFeatureToggleAsync(FeatureToggleInput featureToggleInput)
        {
            try
            {
                if (featureToggleInput is null)
                {
                    _logger.LogError("Feature toggle input is null");
                    return Result<FeatureToggleOutput>.Failure("Feature toggle input cannot be null.");
                }
                var featureToggle = featureToggleInput.ToFeatureToggle();
                var newFeatureToggle = await _repo.AddFeatureToggleAsync(featureToggle);
                return newFeatureToggle.IsSuccess ? Result<FeatureToggleOutput>.Success(newFeatureToggle.Value.ToFeatureToggleOutput()) :
                    Result<FeatureToggleOutput>.Failure(newFeatureToggle.Error);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error adding feature toggle");
                return Result<FeatureToggleOutput>.Failure("An error occurred while adding the feature toggle.");
            }
        }

        public async Task<Result<bool>> DeleteFeatureToggleAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogError("Feature toggle id is empty");
                    return Result<bool>.Failure("Feature toggle id cannot be empty.");
                }
                var result = await _repo.DeleteFeatureToggleAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feature toggle");
                return Result<bool>.Failure("An error occurred while deleting the feature toggle.");
            }
        }

        public async Task<Result<IEnumerable<FeatureToggleOutput>>> GetAllFeatureTogglesAsync()
        {
            try
            {
                var featureToggles = await _repo.GetAllFeatureTogglesAsync();

                if (!featureToggles.IsSuccess)
                    return Result<IEnumerable<FeatureToggleOutput>>.Failure(featureToggles.Error!);

                var featureToggleOutputs = featureToggles.Value!.Select(t => t.ToFeatureToggleOutput());
                return Result<IEnumerable<FeatureToggleOutput>>.Success(featureToggleOutputs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all feature toggles");
                return Result<IEnumerable<FeatureToggleOutput>>.Failure("An error occurred while retrieving feature toggles.");
            }
        }

        public async Task<Result<FeatureToggleOutput>> GetFeatureToggleByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogError("Feature toggle id is empty");
                    return Result<FeatureToggleOutput>.Failure("Feature toggle id cannot be empty.");
                }
                var featureToggle = await _repo.GetFeatureToggleByIdAsync(id);

                if (!featureToggle.IsSuccess || featureToggle.Value == null)
                    return Result<FeatureToggleOutput>.Failure(featureToggle.Error ?? "Feature toggle not found.");

                return Result<FeatureToggleOutput>.Success(featureToggle.Value.ToFeatureToggleOutput());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature toggle by id");
                return Result<FeatureToggleOutput>.Failure("An error occurred while retrieving the feature toggle.");
            }
        }

        public async Task<Result<FeatureToggleOutput>> GetFeatureToggleByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogError("Feature toggle name is empty");
                    return await Task.FromResult(Result<FeatureToggleOutput>.Failure("Feature toggle name cannot be empty."));
                }
                var featureToggle = await _repo.GetFeatureToggleByNameAsync(name);
                if (!featureToggle.IsSuccess || featureToggle.Value == null)
                    return await Task.FromResult(Result<FeatureToggleOutput>.Failure(featureToggle.Error ?? "Feature toggle not found."));

                return Result<FeatureToggleOutput>.Success(featureToggle.Value.ToFeatureToggleOutput());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature toggle by name");
                return Result<FeatureToggleOutput>.Failure("An error occurred while retrieving the feature toggle.");
            }
        }

        public async Task<Result<IEnumerable<FeatureToggleOutput>>> GetFeatureTogglesByNamesAsync(FeatureToggleNameInput input)
        {
            try
            {
                if (input is null || input.Names is null || !input.Names.Any())
                {
                    _logger.LogError("Feature toggle names input is null or empty");
                    return Result<IEnumerable<FeatureToggleOutput>>.Failure("Feature toggle names cannot be null or empty.");
                }
                var featureToggleNames = input.ToFeatureToggleNames();
                var featureToggles = await _repo.GetFeatureTogglesByNamesAsync(featureToggleNames);
                if (!featureToggles.IsSuccess)
                {
                    _logger.LogError("Error retrieving feature toggles by names: {Error}", featureToggles.Error);
                    return Result<IEnumerable<FeatureToggleOutput>>.Failure(featureToggles.Error);
                }
                return Result<IEnumerable<FeatureToggleOutput>>.Success(featureToggles.Value.Select(ft => ft.ToFeatureToggleOutput()));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error checking if feature toggle is enabled");
                return Result<IEnumerable<FeatureToggleOutput>>.Failure("An error occurred while checking if the feature toggle is enabled.");
            }
        }

        public async Task<Result<FeatureToggleOutput>> PatchFeatureToggleAsync(string name, FeatureTogglePatchDto featureTogglePatchDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogError("Feature toggle name is empty");
                    return Result<FeatureToggleOutput>.Failure("Feature toggle name cannot be empty.");
                }
                if (featureTogglePatchDto is null)
                {
                    _logger.LogError("Feature toggle input is null");
                    return Result<FeatureToggleOutput>.Failure("Feature toggle input cannot be null.");
                }
                var updatedFeatureToggle = await _repo.PatchFeatureToggleAsync(name, featureTogglePatchDto);

                return updatedFeatureToggle.IsSuccess && updatedFeatureToggle.Value != null
                    ? Result<FeatureToggleOutput>.Success(updatedFeatureToggle.Value.ToFeatureToggleOutput())
                    : Result<FeatureToggleOutput>.Failure(updatedFeatureToggle.Error ?? "Feature toggle not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching feature toggle");
                return Result<FeatureToggleOutput>.Failure("An error occurred while patching the feature toggle.");
            }
        }
    }
}