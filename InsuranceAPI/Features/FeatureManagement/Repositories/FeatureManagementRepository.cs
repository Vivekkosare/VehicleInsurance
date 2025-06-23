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

            await RemoveCacheForAllFeatureTogglesAsync();

            await RemoveCacheForFeatureTogglesByNamesPrefixAsync();

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

                await RemoveCacheForAllFeatureTogglesAsync();

                await RemoveCacheForFeatureTogglesByNamesPrefixAsync();

                return Result<bool>.Success(true);
            }
            _logger.LogError("Failed to delete feature toggle with ID {FeatureToggleId}.", id);
            return Result<bool>.Failure($"Failed to delete feature toggle with ID {id}.");
        }

        private async Task RemoveCacheForAllFeatureTogglesAsync()
        {
            var cacheKey = "AllFeatureToggles";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Removed cache for all feature toggles with key: {CacheKey}", cacheKey);
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
            var cachedFeatureToggle = await _cache.GetAsync<FeatureToggle?>(cacheKey);
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

        public async Task<Result<FeatureToggle>> PatchFeatureToggleAsync(string name, FeatureTogglePatchDto featureTogglePatchDto)
        {
            var existingToggle = await FetchExistingToggleForPatchAsync(name);
            if (existingToggle == null)
            {
                _logger.LogWarning("Feature toggle with ID {FeatureToggleId} not found for patching.", name);
                return Result<FeatureToggle>.Failure($"Feature toggle with Name {name} not found.");
            }

            bool updated = false;
            // If PatchDescription returns true, updated becomes true and stays true for any subsequent true result
            updated |= PatchDescription(existingToggle, featureTogglePatchDto); // Accumulate if description was changed
            updated |= PatchIsEnabled(existingToggle, featureTogglePatchDto);   // Accumulate if enabled status was changed

            if (updated)
            {
                existingToggle.UpdatedAt = DateTime.UtcNow;

                // Save the patched toggle to the database
                await SavePatchedToggleAsync(existingToggle, name);

                // Fetch existing toggle again to ensure we have the latest state
                var updatedToggle = await FetchExistingToggleForPatchAsync(name);
                if (updatedToggle == null)
                {
                    _logger.LogError("Feature toggle with Name {FeatureToggleName} not found after patching.", name);
                    return Result<FeatureToggle>.Failure($"Feature toggle with Name {name} not found after patching.");
                }
                await UpdateFeatureToggleCacheAsync(updatedToggle);
                _logger.LogInformation("Feature toggle with ID {FeatureToggleId} patched successfully.", updatedToggle.Id);
                return Result<FeatureToggle>.Success(updatedToggle);
            }
            else
            {
                _logger.LogInformation("No changes detected for feature toggle with ID {FeatureToggleId}.", existingToggle.Id);
                return Result<FeatureToggle>.Success(existingToggle);
            }
        }

        /// <summary>
        /// Fetches an existing feature toggle for patching by its name.
        /// </summary>
        /// <param name="name">The name of the feature toggle.</param>
        /// <returns>The feature toggle if found, otherwise null.</returns>
        private async Task<FeatureToggle?> FetchExistingToggleForPatchAsync(string name)
        {
            return await _dbContext.FeatureToggles
                .AsNoTracking()
                .FirstOrDefaultAsync(ft => ft.Name == name);
        }

        private bool PatchDescription(FeatureToggle toggle, FeatureTogglePatchDto patchDto)
        {
            if (patchDto.Description != null && patchDto.Description != toggle.Description)
            {
                toggle.Description = patchDto.Description;
                return true;
            }
            return false;
        }

        private bool PatchIsEnabled(FeatureToggle toggle, FeatureTogglePatchDto patchDto)
        {
            if (patchDto.IsEnabled.HasValue && patchDto.IsEnabled != toggle.IsEnabled)
            {
                toggle.IsEnabled = patchDto.IsEnabled.GetValueOrDefault();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the patched feature toggle to the database.
        /// </summary>
        /// <param name="toggle">The feature toggle to save.</param>
        /// <param name="name">The name of the feature toggle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SavePatchedToggleAsync(FeatureToggle toggle, string name)
        {
            var providerName = _dbContext.Database.ProviderName;
            if (!string.IsNullOrEmpty(providerName) && providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                var trackedToggle = await _dbContext.FeatureToggles.FirstOrDefaultAsync(ft => ft.Name == name);
                if (trackedToggle != null)
                {
                    trackedToggle.Description = toggle.Description;
                    trackedToggle.IsEnabled = toggle.IsEnabled;
                    trackedToggle.UpdatedAt = toggle.UpdatedAt;
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                await _dbContext.FeatureToggles
                    .Where(f => f.Name == name)
                    .ExecuteUpdateAsync(ft => ft
                        .SetProperty(f => f.Description, toggle.Description)
                        .SetProperty(f => f.IsEnabled, toggle.IsEnabled)
                        .SetProperty(f => f.UpdatedAt, toggle.UpdatedAt));
            }
        }

        private async Task UpdateFeatureToggleCacheAsync(FeatureToggle updatedToggle)
        {
            var cacheKeyById = $"FeatureToggle_{updatedToggle.Id}";
            await _cache.SetAsync(cacheKeyById, updatedToggle, _cacheTimeout);

            var cacheKeyByName = $"FeatureToggle_{updatedToggle.Name}";
            await _cache.SetAsync(cacheKeyByName, updatedToggle, _cacheTimeout);

            await RemoveCacheForAllFeatureTogglesAsync();
            await RemoveCacheForFeatureTogglesByNamesPrefixAsync();
            await RemoveCacheForAllInsurancesByInsurancePrefixAsync();
        }

        /// <summary>
        /// Removes the cache for feature toggles status by name prefix.
        /// </summary>
        public async Task RemoveCacheForFeatureTogglesByNamesPrefixAsync()
        {
            var prefix = "FeatureTogglesStatus_";
            // Remove the cache for vehicles by personal identification numbers
            await _cache.ClearByPatternAsync($"{prefix}*");
            _logger.LogInformation($"Removed featureToggles cache with prefix {prefix}.");
        }

        /// <summary>
        /// Removes the cache for all insurances by insurance prefix.
        /// </summary>
        public async Task RemoveCacheForAllInsurancesByInsurancePrefixAsync()
        {
            var prefix = "Insurance";
            // Remove the cache for all insurances by prefix
            await _cache.ClearByPatternAsync($"{prefix}*");
            _logger.LogInformation($"Removed All insurances cache with prefix {prefix}.");

            var allInsuracesCacheKey = "AllInsurances";
            // Remove the cache for vehicles by personal identification numbers
            await _cache.RemoveAsync(allInsuracesCacheKey);
            _logger.LogInformation($"Removed All insurances cache with key {allInsuracesCacheKey}.");
        }

        /// <summary>
        /// Retrieves a feature toggle by its name.
        /// </summary>
        /// <param name="name">The name of the feature toggle.</param>
        /// <returns>A result containing the feature toggle.</returns>
        public async Task<Result<FeatureToggle>> GetFeatureToggleByNameAsync(string name)
        {
            var cacheKey = $"FeatureToggle_{name}";
            // Check cache first
            var cachedFeatureToggle = await _cache.GetAsync<FeatureToggle?>(cacheKey);
            if (cachedFeatureToggle.IsSuccess && cachedFeatureToggle.Value != null)
            {
                _logger.LogInformation("Found feature toggle in cache for key: {CacheKey}", cacheKey);
                return Result<FeatureToggle>.Success(cachedFeatureToggle.Value);
            }
            // If not in cache, retrieve from the database
            var featureToggle = await _dbContext.FeatureToggles
                .AsNoTracking()
                .FirstOrDefaultAsync(ft => ft.Name == name);
            if (featureToggle == null)
            {
                _logger.LogWarning("Feature toggle with Name {FeatureToggleName} not found.", name);
                return Result<FeatureToggle>.Failure($"Feature toggle with Name {name} not found.");
            }
            _logger.LogInformation("Feature toggle with Name {FeatureToggleName} retrieved successfully.", name);
            // Cache the feature toggle for future requests
            await _cache.SetAsync(cacheKey, featureToggle, _cacheTimeout);
            return Result<FeatureToggle>.Success(featureToggle);
        }

        /// <summary>
        /// Retrieves feature toggles by their names.
        /// </summary>
        /// <param name="names">List of feature toggle names.</param>
        /// <returns>A result containing the list of feature toggles.</returns>
        public async Task<Result<IEnumerable<FeatureToggle>>> GetFeatureTogglesByNamesAsync(List<string> names)
        {
            string toggleNames = string.Join(", ", names);
            var cacheKey = $"FeatureToggles_{string.Join("_", names)}";
            // Check cache first
            var cachedResult = await _cache.GetAsync<IEnumerable<FeatureToggle>>(cacheKey);
            if (cachedResult.IsSuccess)
            {
                var value = cachedResult.Value ?? new List<FeatureToggle>();
                _logger.LogInformation("Found feature toggle enabled status in cache for key: {CacheKey}", cacheKey);
                return Result<IEnumerable<FeatureToggle>>.Success(value);
            }
            // If not in cache, retrieve from the database
            if (names == null)
            {
                _logger.LogWarning("Names list is null in GetFeatureTogglesByNamesAsync.");
                return Result<IEnumerable<FeatureToggle>>.Failure("Names list cannot be null.");
            }
            var featureToggles = await _dbContext.FeatureToggles
                .AsNoTracking()
                .Where(ft => ft.Name != null && names.Contains(ft.Name))
                .ToListAsync();

            if (featureToggles == null || !featureToggles.Any())
            {
                _logger.LogWarning("Feature toggle(s) with Name(s) {FeatureToggleName} not found.", toggleNames);
                return Result<IEnumerable<FeatureToggle>>.Failure($"Feature toggle(s) with Name(s) {toggleNames} not found.");
            }
            _logger.LogInformation("Feature toggle(s) with Name(s) {FeatureToggleName} retrieved successfully.", toggleNames);
            // Cache the result
            await _cache.SetAsync(cacheKey, featureToggles, _cacheTimeout);
            return Result<IEnumerable<FeatureToggle>>.Success(featureToggles);
        }
    }
}