using InsuranceAPI.Data;
using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Entities;
using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Shared.DTOs;
using VehicleInsurance.Shared.Services;

namespace InsuranceAPI.Features.FeatureManagement.Repositories
{
    public class FeatureManagementRepository(InsuranceDbContext _dbContext,
    ICacheService _cache,
    ILogger<FeatureManagementRepository> _logger,
    IConfiguration _config) : IFeatureManagementRepository
    {
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(int.TryParse(_config["TimeoutInMinutes"], out var t) ? t : 5);
        public async Task<Result<FeatureToggle>> AddFeatureToggleAsync(FeatureToggle featureToggle)
        {
            var cacheKey = $"FeatureToggle_{featureToggle.Name}";
            // Check if the feature toggle is already cached
            var cachedFeatureToggle = await _cache.GetAsync<FeatureToggle>(cacheKey);
            if (cachedFeatureToggle.IsSuccess && cachedFeatureToggle.Value != null)
            {
                _logger.LogWarning("Feature toggle with ID {FeatureToggleId} already exists in cache.", featureToggle.Id);
                return Result<FeatureToggle>.Failure("Feature toggle already exists.");
            }
            var featureToggleExists = await FeatureToggleExistsAsync(featureToggle.Name);
            if (featureToggleExists.IsSuccess && featureToggleExists.Value)
            {
                _logger.LogWarning("Feature toggle with ID {FeatureToggleId} already exists.", featureToggle.Id);
                return Result<FeatureToggle>.Failure("Feature toggle already exists.");
            }
            var newFeatureToggle = await _dbContext.FeatureToggles.AddAsync(featureToggle);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Feature toggle with ID {FeatureToggleId} created successfully.", newFeatureToggle.Entity.Id);

            // Cache the newly created feature toggle
            await _cache.SetAsync(cacheKey, newFeatureToggle.Entity, _cacheTimeout);
            return Result<FeatureToggle>.Success(newFeatureToggle.Entity);
            
        }

        private async Task<Result<bool>> FeatureToggleExistsAsync(string name)
        {
            var cacheKey = $"FeatureToggleExists_{name}";

            //Check if the result is cached
            var cachedResult = await _cache.GetAsync<bool?>(cacheKey);
            if (cachedResult.IsSuccess && cachedResult.Value.HasValue)
            {
                return Result<bool>.Success(cachedResult.Value.Value);
            }

            // If not cached, Check if the feature toggle exists in the database
            var exists = await _dbContext.FeatureToggles.AnyAsync(ft => ft.Name == name);
            // Cache the result
            await _cache.SetAsync(cacheKey, exists, _cacheTimeout);
            return Result<bool>.Success(exists);
        }

        public async Task<Result<bool>> DeleteFeatureToggleAsync(Guid id)
        {
            var featureToggle = await GetFeatureToggleByIdAsync(id);
            if (!featureToggle.IsSuccess || featureToggle.Value == null)
            {
                _logger.LogWarning("Feature toggle with ID {FeatureToggleId} not found.", id);
                return Result<bool>.Failure($"Feature toggle with ID {id} not found.");
            }
            _dbContext.FeatureToggles.Remove(featureToggle.Value);
            var result = await _dbContext.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Feature toggle with ID {FeatureToggleId} deleted successfully.", id);

                // Remove from cache
                var cacheKeyById = $"FeatureToggle_{id}";
                await _cache.RemoveAsync(cacheKeyById);

                var cacheKeyByName = $"FeatureToggle_{featureToggle.Value.Name}";
                await _cache.RemoveAsync(cacheKeyByName);

                var cacheKeyEnabled = $"FeatureToggleEnabled_{featureToggle.Value.Name}";
                await _cache.RemoveAsync(cacheKeyEnabled);

                return Result<bool>.Success(true);
            }
            _logger.LogError("Failed to delete feature toggle with ID {FeatureToggleId}.", id);
            return Result<bool>.Failure($"Failed to delete feature toggle with ID {id}.");
        }

        public async Task<Result<IEnumerable<FeatureToggle>>> GetAllFeatureTogglesAsync()
        {
            var cacheKey = "AllFeatureToggles";
            // Check cache first
            var cachedFeatureToggles = await _cache.GetAsync<IEnumerable<FeatureToggle>>(cacheKey);
            if (cachedFeatureToggles.IsSuccess && cachedFeatureToggles.Value != null)
            {
                _logger.LogInformation("Found feature toggles in cache for key: {CacheKey}", cacheKey);
                return Result<IEnumerable<FeatureToggle>>.Success(cachedFeatureToggles.Value);
            }
            // If not in cache, retrieve from the database
            var featureToggles = await _dbContext.FeatureToggles.ToListAsync();
            if (featureToggles == null || !featureToggles.Any())
            {
                _logger.LogWarning("No feature toggles found in the database.");
                return Result<IEnumerable<FeatureToggle>>.Success(Enumerable.Empty<FeatureToggle>());
            }
            _logger.LogInformation("Retrieved {Count} feature toggles from the database.", featureToggles.Count);
            // Cache the feature toggles for future requests
            await _cache.SetAsync(cacheKey, featureToggles, _cacheTimeout);
            
            return Result<IEnumerable<FeatureToggle>>.Success(featureToggles);
        }

        public async Task<Result<FeatureToggle?>> GetFeatureToggleByIdAsync(Guid id)
        {
            //check cache first
            var cacheKey = $"FeatureToggle_{id}";
            var cachedFeatureToggle = await _cache.GetAsync<FeatureToggle>(cacheKey);
            if (cachedFeatureToggle.IsSuccess && cachedFeatureToggle.Value != null)
            {
                _logger.LogInformation("Found feature toggle in cache for key: {CacheKey}", cacheKey);
                return Result<FeatureToggle?>.Success(cachedFeatureToggle.Value);
            }

            // If not in cache, retrieve from the database
            var featureToggle = await _dbContext.FeatureToggles.FindAsync(id);
            if (featureToggle == null)
            {
                _logger.LogWarning("Feature toggle with ID {FeatureToggleId} not found.", id);
                return Result<FeatureToggle?>.Failure($"Feature toggle with ID {id} not found.");
            }
            _logger.LogInformation("Feature toggle with ID {FeatureToggleId} retrieved successfully.", id);
            // Cache the feature toggle for future requests
            await _cache.SetAsync(cacheKey, featureToggle, _cacheTimeout);
            return Result<FeatureToggle?>.Success(featureToggle);
        }

        public async Task<Result<FeatureToggle>> PatchFeatureToggleAsync(Guid id, FeatureToggle featureToggle)
        {
            var existingToggleResult = await GetFeatureToggleByIdAsync(id);
            if (!existingToggleResult.IsSuccess || existingToggleResult.Value == null)
            {
                _logger.LogWarning("Feature toggle with ID {FeatureToggleId} not found for patching.", id);
                return Result<FeatureToggle>.Failure($"Feature toggle with ID {id} not found.");
            }
            var existingToggle = existingToggleResult.Value;

            // If name is being changed, check for uniqueness first
            if (!string.IsNullOrWhiteSpace(featureToggle.Name) && featureToggle.Name != existingToggle.Name)
            {
                var nameExists = await _dbContext.FeatureToggles
                    .AnyAsync(ft => ft.Name == featureToggle.Name && ft.Id != id);
                if (nameExists)
                {
                    _logger.LogWarning("Feature toggle with Name {FeatureToggleName} already exists.", featureToggle.Name);
                    return Result<FeatureToggle>.Failure($"Feature toggle with Name {featureToggle.Name} already exists.");
                }
            }

            bool updated = false;
            // Patch Name if changed and not null/empty
            if (!string.IsNullOrWhiteSpace(featureToggle.Name) && featureToggle.Name != existingToggle.Name)
            {
                existingToggle.Name = featureToggle.Name;
                updated = true;
            }
            // Patch Description if changed
            if (featureToggle.Description != null && featureToggle.Description != existingToggle.Description)
            {
                existingToggle.Description = featureToggle.Description;
                updated = true;
            }
            // Patch IsEnabled if changed
            if (featureToggle.IsEnabled != existingToggle.IsEnabled)
            {
                existingToggle.IsEnabled = featureToggle.IsEnabled;
                updated = true;
            }
            if (updated)
            {
                existingToggle.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                // Update cache
                var cacheKeyById = $"FeatureToggle_{existingToggle.Id}";
                await _cache.SetAsync(cacheKeyById, existingToggle, _cacheTimeout);

                var cacheKeyByName = $"FeatureToggle_{existingToggle.Name}";
                await _cache.SetAsync(cacheKeyByName, existingToggle, _cacheTimeout);

                var cacheKeyEnabled = $"FeatureToggleEnabled_{existingToggle.Name}";
                await _cache.SetAsync(cacheKeyEnabled, existingToggle.IsEnabled, _cacheTimeout);

                _logger.LogInformation("Feature toggle with ID {FeatureToggleId} patched successfully.", existingToggle.Id);
            }
            else
            {
                _logger.LogInformation("No changes detected for feature toggle with ID {FeatureToggleId}.", existingToggle.Id);
            }
            return Result<FeatureToggle>.Success(existingToggle);
        }

        public async Task<Result<bool>> IsFeatureToggleEnabledAsync(FeatureToggleNameInput input)
        {
            var cacheKey = $"FeatureToggleEnabled_{string.Join("_", input.Names)}";
            // Check cache first
            var cachedResult = await _cache.GetAsync<bool>(cacheKey);
            if (cachedResult.IsSuccess)
            {
                _logger.LogInformation("Found feature toggle enabled status in cache for key: {CacheKey}", cacheKey);
                return Result<bool>.Success(cachedResult.Value);
            }
            // If not in cache, retrieve from the database
            var featureToggle = await _dbContext.FeatureToggles
                .AsNoTracking()
                .FirstOrDefaultAsync(ft => ft.Name == name);
            if (featureToggle == null)
            {
                _logger.LogWarning("Feature toggle with Name {FeatureToggleName} not found.", name);
                return Result<bool>.Failure($"Feature toggle with Name {name} not found.");
            }
            _logger.LogInformation("Feature toggle with Name {FeatureToggleName} retrieved successfully.", name);
            // Cache the result
            await _cache.SetAsync(cacheKey, featureToggle.IsEnabled, _cacheTimeout);
            return Result<bool>.Success(featureToggle.IsEnabled);
        }
    }
}