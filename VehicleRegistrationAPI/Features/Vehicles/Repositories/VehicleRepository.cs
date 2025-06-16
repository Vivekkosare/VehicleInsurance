using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Shared;
using VehicleInsurance.Shared.DTOs;
using VehicleInsurance.Shared.Services;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Entities;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Repositories;

public class VehicleRepository(VehicleRegistrationDbContext dbContext,
ILogger<VehicleRepository> _logger,
ICacheService _cache) : IVehicleRepository
{
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
    public async Task<Result<Vehicle>> AddVehicleAsync(Vehicle vehicle)
    {
        var newVehicle = await dbContext.Vehicles.AddAsync(vehicle);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation($"Vehicle with registration number {newVehicle.Entity.RegistrationNumber} created successfully.");

        //Reload the Owner and Vehicle entities to ensure they are up-to-date
        await dbContext.Entry(newVehicle.Entity).ReloadAsync();
        dbContext.Entry(newVehicle.Entity).Reference(v => v.Owner).Load();

        var cacheKey = $"Vehicle_{newVehicle.Entity.RegistrationNumber}";
        await _cache.SetAsync(cacheKey, newVehicle.Entity, _cacheTimeout);
        _logger.LogInformation($"Cached vehicle with registration number {newVehicle.Entity.RegistrationNumber}.");

        if (!string.IsNullOrWhiteSpace(newVehicle.Entity.Owner?.PersonalIdentificationNumber))
        {
            await RemoveCacheForVehiclesByPersonalIdentificationNumberAsync(newVehicle.Entity.Owner.PersonalIdentificationNumber);
        }

        // Remove the cache for vehicles by personal identification numbers
        await RemoveCacheForVehiclesByPersondIdsPrefixAsync();

        //Remove the cache for all vehicles
        await RemoveCacheForAllVehiclesAsync();

        return Result<Vehicle>.Success(newVehicle.Entity);
    }
    public async Task<Result<bool>> SetVehicleExitsCacheAsync(string registrationNumber, bool exists)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            _logger.LogError("Registration number cannot be null or empty.");
            return Result<bool>.Failure("Registration number cannot be null or empty.");
        }
        var cacheKey = $"VehicleExists_{registrationNumber}";
        await _cache.SetAsync(cacheKey, exists, _cacheTimeout);
        _logger.LogInformation($"Cached vehicle existence for registration number {registrationNumber} as {exists}.");
        return Result<bool>.Success(true);
    }
    public async Task<Result<bool>> RemoveVehicleCacheAsync(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            _logger.LogError("Vehicle cannot be null.");
            return Result<bool>.Failure("Vehicle cannot be null.");
        }

        var cacheKeyVehiclExists = $"VehicleExists_{vehicle.RegistrationNumber}";
        await _cache.RemoveAsync(cacheKeyVehiclExists);
        _logger.LogInformation($"Removed vehicle existence cache by registration number {vehicle.RegistrationNumber}.");


        var cacheKeyRegistrationNo = $"Vehicle_{vehicle.RegistrationNumber}"; ;
        await _cache.RemoveAsync(cacheKeyRegistrationNo);
        _logger.LogInformation($"Removed vehicle cache by registration number {vehicle.RegistrationNumber}.");


        var cacheKeyVehicleId = $"Vehicle_{vehicle.Id}"; ;
        await _cache.RemoveAsync(cacheKeyVehicleId);
        _logger.LogInformation($"Removed vehicle cache by cacheKeyVehicleId {vehicle.Id}.");

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteVehicleAsync(Guid vehicleId)
    {
        var vehicle = await GetVehicleByIdAsync(vehicleId);
        if (!vehicle.IsSuccess || vehicle.Value == null)
        {
            _logger.LogError($"Vehicle with ID {vehicleId} not found for deletion.");
            return Result<bool>.Failure($"Vehicle with ID {vehicleId} not found.");
        }

        dbContext.Vehicles.Remove(vehicle.Value);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Vehicle with ID {vehicleId} deleted successfully.");

        // Remove the vehicle from cache
        await RemoveVehicleCacheAsync(vehicle.Value);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetAllVehiclesAsync()
    {
        var cacheKey = "AllVehicles";
        // Check if the vehicles exist in the cache first
        var cachedVehicles = await _cache.GetAsync<IEnumerable<Vehicle>>(cacheKey);
        if (cachedVehicles.IsSuccess && cachedVehicles.Value != null)
        {
            _logger.LogInformation("All vehicles retrieved from cache.");
            return Result<IEnumerable<Vehicle>>.Success(cachedVehicles.Value); // Vehicles exist in cache
        }
        // If not in cache, check the database
        _logger.LogInformation("Retrieving all vehicles from the database.");

        // Load all vehicles with their owners
        var vehicles = await dbContext.Vehicles
                        .Include(v => v.Owner).ToListAsync();
        if (vehicles == null || !vehicles.Any())
        {
            _logger.LogWarning("No vehicles found in the database.");
            return Result<IEnumerable<Vehicle>>.Success(new List<Vehicle>());
        }
        // Cache the vehicles for future requests
        await _cache.SetAsync(cacheKey, vehicles, _cacheTimeout);
        _logger.LogInformation($"Cached {vehicles.Count} vehicles for future requests.");

        return Result<IEnumerable<Vehicle>>.Success(vehicles);
    }

    public async Task<Result<Vehicle>> GetVehicleByIdAsync(Guid vehicleId)
    {
        var cacheKey = $"Vehicle_{vehicleId}";
        // Check if the vehicle exists in the cache first
        var cachedVehicle = await _cache.GetAsync<Vehicle>(cacheKey);
        if (cachedVehicle.IsSuccess && cachedVehicle.Value != null)
        {
            _logger.LogInformation($"Vehicle with ID {vehicleId} retrieved from cache.");
            return Result<Vehicle>.Success(cachedVehicle.Value); // Vehicle exists in cache
        }
        // If not in cache, check the database
        _logger.LogInformation($"Retrieving vehicle with ID {vehicleId} from the database.");
        var vehicle = await dbContext.Vehicles.FindAsync(vehicleId);
        if (vehicle == null)
        {
            _logger.LogError($"Vehicle with ID {vehicleId} not found.");
            return Result<Vehicle>.Failure($"Vehicle with ID {vehicleId} not found.");
        }
        await dbContext.Entry(vehicle).ReloadAsync();
        dbContext.Entry(vehicle).Reference(v => v.Owner).Load();
        _logger.LogInformation($"Vehicle with ID {vehicleId} retrieved successfully.");

        // Cache the vehicle for future requests
        await _cache.SetAsync(cacheKey, vehicle, _cacheTimeout);
        _logger.LogInformation($"Cached vehicle with ID {vehicleId}.");
        if (vehicle.Owner != null)
        {
            await dbContext.Entry(vehicle.Owner).ReloadAsync();
        }
        _logger.LogInformation($"Vehicle with ID {vehicleId} has owner {vehicle.Owner?.Name}.");

        return Result<Vehicle>.Success(vehicle);
    }

    public async Task<Result<Vehicle>> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            _logger.LogError("Registration number cannot be null or empty.");
            return Result<Vehicle>.Failure("Registration number cannot be null or empty.");
        }
        // Check if the vehicle exists in the cache first
        var cacheKey = $"Vehicle_{registrationNumber}";
        var cachedVehicle = await _cache.GetAsync<Vehicle>(cacheKey);
        if (cachedVehicle.IsSuccess && cachedVehicle.Value != null)
        {
            _logger.LogInformation($"Vehicle with registration number {registrationNumber} retrieved from cache.");
            return Result<Vehicle>.Success(cachedVehicle.Value); // Vehicle exists in cache
        }
        // If not in cache, check the database
        _logger.LogInformation($"Retrieving vehicle with registration number {registrationNumber} from the database.");

        var vehicle = await dbContext.Vehicles
                        .Include(v => v.Owner)
                        .FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber);

        if (vehicle == null)
        {
            _logger.LogError($"Vehicle with registration number {registrationNumber} not found.");
            return Result<Vehicle>.Failure($"Vehicle with registration number {registrationNumber} not found.");
        }
        // Cache the vehicle for future requests
        await _cache.SetAsync(cacheKey, vehicle, _cacheTimeout);
        _logger.LogInformation($"Cached vehicle with registration number {registrationNumber}.");
        if (vehicle.Owner != null)
        {
            await dbContext.Entry(vehicle.Owner).ReloadAsync();
        }
        _logger.LogInformation($"Vehicle with registration number {registrationNumber} has owner {vehicle.Owner?.Name}.");
        return Result<Vehicle>.Success(vehicle);
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetVehiclesByCustomerIdAsync(Guid customerId)
    {
        var cacheKey = $"VehiclesByCustomer_{customerId}";
        // Check if the vehicles exist in the cache first
        var cachedVehicles = await _cache.GetAsync<IEnumerable<Vehicle>>(cacheKey);
        if (cachedVehicles.IsSuccess && cachedVehicles.Value != null)
        {
            _logger.LogInformation($"Vehicles for customer ID {customerId} retrieved from cache.");
            return Result<IEnumerable<Vehicle>>.Success(cachedVehicles.Value); // Vehicles exist in cache
        }
        // If not in cache, check the database
        _logger.LogInformation($"Retrieving vehicles for customer ID {customerId} from the database.");

        var vehicles = await dbContext.Vehicles
            .Where(v => v.OwnerId == customerId)
            .ToListAsync();
        if (vehicles == null || !vehicles.Any())
        {
            _logger.LogWarning($"No vehicles found for customer ID {customerId}.");
            return Result<IEnumerable<Vehicle>>.Success(new List<Vehicle>());
        }
        // Cache the vehicles for future requests
        await _cache.SetAsync(cacheKey, vehicles, _cacheTimeout);
        _logger.LogInformation($"Cached vehicles for customer ID {customerId}.");
        _logger.LogInformation($"Retrieved {vehicles.Count} vehicles for customer ID {customerId}.");

        return Result<IEnumerable<Vehicle>>.Success(vehicles);
    }

    public async Task<Result<bool>> UpdateVehicleAsync(Guid vehicleId, Vehicle vehicle)
    {
        var existingVehicle = await GetVehicleByIdAsync(vehicleId);
        if (!existingVehicle.IsSuccess || existingVehicle.Value == null)
        {
            _logger.LogError($"Vehicle with ID {vehicleId} not found for update.");
            return Result<bool>.Failure($"Vehicle with ID {vehicleId} not found.");
        }
        await dbContext.Entry(existingVehicle.Value).ReloadAsync();
        dbContext.Entry(existingVehicle.Value).CurrentValues.SetValues(vehicle);
        // If you need to update navigation properties, you can do so here

        dbContext.Vehicles.Update(existingVehicle.Value);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Vehicle with ID {vehicleId} updated successfully.");

        // Update the cache with the new vehicle data
        var cacheKey = $"Vehicle_{vehicleId}";
        await _cache.SetAsync(cacheKey, existingVehicle.Value, _cacheTimeout);
        _logger.LogInformation($"Cached updated vehicle with ID {vehicleId}.");

        // Update the cache by Vehicle Id and Registration Number
        await UpdateCacheForVehicleAsync(existingVehicle.Value);

        if (!string.IsNullOrWhiteSpace(existingVehicle.Value.Owner?.PersonalIdentificationNumber))
        {
            await RemoveCacheForVehiclesByPersonalIdentificationNumberAsync(existingVehicle.Value.Owner.PersonalIdentificationNumber);
        }

        // Remove the cache for vehicles by personal identification numbers
        await RemoveCacheForVehiclesByPersondIdsPrefixAsync();


        return Result<bool>.Success(true);


    }

    /// <summary>
    /// Updates the cache for a specific vehicle by its ID and registration number.
    /// </summary>
    public async Task UpdateCacheForVehicleAsync(Vehicle vehicle)
    {

        // Update the cache with the new vehicle data
        var cacheKey = $"Vehicle_{vehicle.Id}";
        await _cache.SetAsync(cacheKey, vehicle, _cacheTimeout);
        _logger.LogInformation($"Cached updated vehicle with ID {vehicle.Id}.");

        // Update the cache By Registration No. with the new vehicle data
        var cacheKeyByRegistrationNo = $"Vehicle_{vehicle.RegistrationNumber}";
        await _cache.SetAsync(cacheKeyByRegistrationNo, vehicle, _cacheTimeout);
        _logger.LogInformation($"Cached updated vehicle by RegistrationNo {vehicle.RegistrationNumber}.");

    }

    /// <summary>
    /// Removes the cache for a specific vehicle by Personal Identification Number.
    public async Task RemoveCacheForVehiclesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        if (string.IsNullOrWhiteSpace(personalIdentificationNumber))
        {
            _logger.LogError("Personal identification number cannot be null or empty.");
            return;
        }
        // Remove the cache for vehicles by personal identification number
        var cacheKey = $"VehiclesByPIN_{personalIdentificationNumber}";
        await _cache.RemoveAsync(cacheKey);
        _logger.LogInformation($"Removed VehiclesByPIN cache for personal identification number {personalIdentificationNumber}.");
    }

    /// <summary>
    /// Removes the cache for vehicles by personal identification numbers prefix.
    /// </summary>
    public async Task RemoveCacheForVehiclesByPersondIdsPrefixAsync()
    {
        var prefix = "VehiclesByPersonIds_";
        // Remove the cache for vehicles by personal identification numbers
        await _cache.ClearByPatternAsync($"{prefix}*");
        _logger.LogInformation($"Removed VehiclesByPersonIds cache with prefix {prefix}.");
    }

    /// <summary>
    /// Removes the cache for all vehicles.
    /// </summary>
    public async Task RemoveCacheForAllVehiclesAsync()
    {
        // Remove the cache for all vehicles
        var cacheKey = "AllVehicles";
        await _cache.RemoveAsync(cacheKey);
        _logger.LogInformation("Removed AllVehicles cache.");
    }

    public async Task<Result<bool>> VehicleExistsAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            _logger.LogError("Registration number cannot be null or empty.");
        }
        // Check if the vehicle exists in the cache first
        var cacheKey = $"VehicleExists_{registrationNumber}";
        var vehicleExistsInCache = await _cache.GetAsync<bool>(cacheKey);
        if (vehicleExistsInCache.IsSuccess && vehicleExistsInCache.Value)
        {
            return Result<bool>.Success(vehicleExistsInCache.Value); // Vehicle exists in cache
        }
        // If not in cache, check the database
        _logger.LogInformation($"Checking if vehicle with registration number {registrationNumber} exists in the database.");
        bool vehicleExists = await dbContext.Vehicles
                            .AnyAsync(v => v.RegistrationNumber == registrationNumber);
        await _cache.SetAsync(cacheKey, vehicleExists, _cacheTimeout);

        _logger.LogInformation($"Vehicle with registration number {registrationNumber} exists: {vehicleExists}");
        return Result<bool>.Success(vehicleExists);
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetVehiclesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        if (string.IsNullOrWhiteSpace(personalIdentificationNumber))
        {
            _logger.LogError("Personal identification number cannot be null or empty.");
        }
        // Check if the vehicles exist in the cache first
        var cacheKey = $"VehiclesByPIN_{personalIdentificationNumber}";
        var vehiclesInCache = await _cache.GetAsync<IEnumerable<Vehicle>>(cacheKey);
        if (vehiclesInCache.IsSuccess && vehiclesInCache.Value != null)
        {
            return Result<IEnumerable<Vehicle>>.Success(vehiclesInCache.Value); // Vehicles exist in cache
        }
        // If not in cache, check the database
        _logger.LogInformation($"Retrieving vehicles for personal identification number {personalIdentificationNumber} from the database.");

        var vehiclesByOwner = await dbContext.Vehicles
            .Include(v => v.Owner)
            .Where(v => v.Owner.PersonalIdentificationNumber == personalIdentificationNumber)
            .ToListAsync();
        if (vehiclesByOwner == null || !vehiclesByOwner.Any())
        {
            _logger.LogWarning($"No vehicles found for personal identification number {personalIdentificationNumber}.");
            return Result<IEnumerable<Vehicle>>.Success(new List<Vehicle>());
        }
        // Cache the vehicles for future requests
        await _cache.SetAsync(cacheKey, vehiclesByOwner, _cacheTimeout);
        _logger.LogInformation($"Cached vehicles for personal identification number {personalIdentificationNumber}.");

        return Result<IEnumerable<Vehicle>>.Success(vehiclesByOwner);
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetVehiclesByPersonalIdsAsync(VehicleInsurance.Shared.DTOs.PersonIdentifiersRequest personIds)
    {
        if (personIds == null || personIds.PersonalIdentificationNumbers == null || !personIds.PersonalIdentificationNumbers.Any())
        {
            _logger.LogError("Personal identification numbers cannot be null or empty.");
        }
        // Check if the vehicles exist in the cache first
        var cacheKey = $"VehiclesByPersonIds_{string.Join(",", personIds.PersonalIdentificationNumbers)}";
        var vehiclesInCache = await _cache.GetAsync<IEnumerable<Vehicle>>(cacheKey);
        if (vehiclesInCache.IsSuccess && vehiclesInCache.Value != null)
        {
            return Result<IEnumerable<Vehicle>>.Success(vehiclesInCache.Value); // Vehicles exist in cache
        }
        // If not in cache, check the database
        _logger.LogInformation($"Retrieving vehicles for personal identification numbers {string.Join(", ", personIds.PersonalIdentificationNumbers)} from the database.");
        var vehiclesByOwners = await dbContext.Vehicles
            .Include(v => v.Owner)
            .Where(v => personIds.PersonalIdentificationNumbers.Contains(v.Owner.PersonalIdentificationNumber))
            .ToListAsync();
        if (vehiclesByOwners == null || !vehiclesByOwners.Any())
        {
            _logger.LogWarning($"No vehicles found for personal identification numbers {string.Join(", ", personIds.PersonalIdentificationNumbers)}.");
            return Result<IEnumerable<Vehicle>>.Success(new List<Vehicle>());
        }
        // Cache the vehicles for future requests
        await _cache.SetAsync(cacheKey, vehiclesByOwners, _cacheTimeout);
        _logger.LogInformation($"Cached vehicles for personal identification numbers {string.Join(", ", personIds.PersonalIdentificationNumbers)}.");

        return Result<IEnumerable<Vehicle>>.Success(vehiclesByOwners);
    }
}
