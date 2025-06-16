using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.Extensions;
using VehicleRegistrationAPI.Features.Vehicles.Repositories;

namespace VehicleRegistrationAPI.Features.Vehicles.Services;

public class VehicleService(IVehicleRepository _vehicleRepo,
ILogger<VehicleService> _logger) : IVehicleService
{
    public async Task<Result<VehicleOutput>> AddVehicleAsync(VehicleInput vehicleInput)
    {
        if (vehicleInput == null)
        {
            _logger.LogError("Vehicle input cannot be null.");
            return Result<VehicleOutput>.Failure("Vehicle input cannot be null.");
        }
        var vehicle = vehicleInput.ToVehicleEntity();

        //Check if the vehicle already exists
        var vehicleExists = await VehicleExistsAsync(vehicle.RegistrationNumber);
        if (!vehicleExists.IsSuccess || vehicleExists.Value)
        {
            _logger.LogError($"Vehicle with registration number {vehicle.RegistrationNumber} already exists.");
            return Result<VehicleOutput>.Failure($"Vehicle with registration number {vehicle.RegistrationNumber} already exists.");
        }

        // Add the vehicle to the repository
        var addedVehicle = await _vehicleRepo.AddVehicleAsync(vehicle);
        if (!addedVehicle.IsSuccess)
        {
            _logger.LogError($"Failed to add vehicle: {addedVehicle.Error}");
            return Result<VehicleOutput>.Failure(addedVehicle.Error);
        }
        _logger.LogInformation($"Vehicle with ID {addedVehicle.Value.Id} added successfully.");

        //update the cache if vehicle exists
        var cacheResult = await _vehicleRepo.SetVehicleExitsCacheAsync(addedVehicle.Value.RegistrationNumber, true);
        if (!cacheResult.IsSuccess)
        {
            _logger.LogWarning($"Failed to update cache for vehicle with registration number {addedVehicle.Value.RegistrationNumber}: {cacheResult.Error}");
        }

        // Convert the added vehicle to VehicleOutput
        return Result<VehicleOutput>.Success(addedVehicle.Value.ToVehicleOutput());
    }

    public async Task<Result<bool>> DeleteVehicleAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            _logger.LogError("Vehicle ID cannot be empty.");
            return Result<bool>.Failure("Vehicle ID cannot be empty.");
        }
        var vehicleDeleted = await _vehicleRepo.DeleteVehicleAsync(vehicleId);
        if (!vehicleDeleted.IsSuccess)
        {
            _logger.LogError($"Failed to delete vehicle with ID {vehicleId}: {vehicleDeleted.Error}");
            return Result<bool>.Failure(vehicleDeleted.Error);
        }
        _logger.LogInformation($"Vehicle with ID {vehicleId} deleted successfully.");
        return Result<bool>.Success(vehicleDeleted.Value);
    }

    public async Task<Result<IEnumerable<VehicleOutput>>> GetAllVehiclesAsync()
    {
        var vehicles = await _vehicleRepo.GetAllVehiclesAsync();
        if (vehicles == null || !vehicles.IsSuccess || !vehicles.Value.Any())
        {
            _logger.LogInformation("No vehicles found.");
            return Result<IEnumerable<VehicleOutput>>.Success(Enumerable.Empty<VehicleOutput>());
        }
        _logger.LogInformation($"{vehicles.Value.Count()} vehicles retrieved successfully.");

        return Result<IEnumerable<VehicleOutput>>.Success(vehicles.Value.Select(v => v.ToVehicleOutput()));
    }

    public async Task<Result<VehicleOutput>> GetVehicleByIdAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            _logger.LogError("Vehicle ID cannot be empty.");
            return Result<VehicleOutput>.Failure("Vehicle ID cannot be empty.");
        }
        var vehicle = await _vehicleRepo.GetVehicleByIdAsync(vehicleId);
        if (vehicle == null || !vehicle.IsSuccess)
        {
            _logger.LogError($"Vehicle with ID {vehicleId} not found.");
            return Result<VehicleOutput>.Failure($"Vehicle with ID {vehicleId} not found.");
        }
        _logger.LogInformation($"Vehicle with ID {vehicleId} retrieved successfully.");
        return Result<VehicleOutput>.Success(vehicle.Value.ToVehicleOutput());
    }

    public async Task<Result<VehicleOutput>> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            _logger.LogError("Registration number cannot be null or empty.");
            throw new ArgumentException("Registration number cannot be null or empty.", nameof(registrationNumber));
        }
        var vehicle = await _vehicleRepo.GetVehicleByRegistrationNumberAsync(registrationNumber);
        if (vehicle == null || !vehicle.IsSuccess)
        {
            _logger.LogError($"Vehicle with registration number {registrationNumber} not found.");
            return Result<VehicleOutput>.Failure($"Vehicle with registration number {registrationNumber} not found.");
        }
        _logger.LogInformation($"Vehicle with registration number {registrationNumber} retrieved successfully.");
        return Result<VehicleOutput>.Success(vehicle.Value.ToVehicleOutput());
    }

    public async Task<Result<IEnumerable<VehicleOutput>>> GetVehiclesByCustomerIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogError("Customer ID cannot be empty.");
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }
        var vehicle = await _vehicleRepo.GetVehiclesByCustomerIdAsync(customerId);
        if (vehicle == null || !vehicle.IsSuccess || !vehicle.Value.Any())
        {
            _logger.LogInformation($"No vehicles found for customer ID {customerId}.");
            return Result<IEnumerable<VehicleOutput>>.Success(Enumerable.Empty<VehicleOutput>());
        }
        _logger.LogInformation($"{vehicle.Value.Count()} vehicles retrieved for customer ID {customerId}.");

        return Result<IEnumerable<VehicleOutput>>.Success(vehicle.Value.Select(v => v.ToVehicleOutput()));
    }

    public async Task<Result<VehicleOutput>> UpdateVehicleAsync(Guid vehicleId, VehicleInput vehicleInput)
    {
        if (vehicleId == Guid.Empty)
        {
            _logger.LogError("Vehicle ID cannot be empty.");
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }
        if (vehicleInput == null)
        {
            _logger.LogError("Vehicle input cannot be null.");
            throw new ArgumentNullException(nameof(vehicleInput), "Vehicle input cannot be null.");
        }
        var vehicle = vehicleInput.ToVehicleEntity();
        var vehicleUpdated = await _vehicleRepo.UpdateVehicleAsync(vehicleId, vehicle);
        if (!vehicleUpdated.IsSuccess)
        {
            _logger.LogError($"Failed to update vehicle with ID {vehicleId}: {vehicleUpdated.Error}");
            return Result<VehicleOutput>.Failure(vehicleUpdated.Error);
        }
        _logger.LogInformation($"Vehicle with ID {vehicleId} updated successfully.");

        // Retrieve the updated vehicle from the repository
        var updatedVehicle = await _vehicleRepo.GetVehicleByIdAsync(vehicleId);
        if (updatedVehicle == null || !updatedVehicle.IsSuccess)
        {
            _logger.LogError($"Vehicle with ID {vehicleId} not found after update.");
            return Result<VehicleOutput>.Failure($"Vehicle with ID {vehicleId} not found after update.");
        }
        _logger.LogInformation($"Vehicle with ID {vehicleId} retrieved successfully after update.");

        return Result<VehicleOutput>.Success(updatedVehicle.Value.ToVehicleOutput());
    }

    public async Task<Result<bool>> VehicleExistsAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            _logger.LogError("Registration number cannot be null or empty.");
            return Result<bool>.Failure("Registration number cannot be null or empty.");
        }
        var vehicleExists = await _vehicleRepo.VehicleExistsAsync(registrationNumber);
        if (!vehicleExists.IsSuccess)
        {
            _logger.LogError($"Error checking existence of vehicle with registration number {registrationNumber}: {vehicleExists.Error}");
            return Result<bool>.Failure(vehicleExists.Error);
        }
        _logger.LogInformation($"Vehicle with registration number {registrationNumber} exists: {vehicleExists.Value}");
        return Result<bool>.Success(vehicleExists.Value);
    }

    public async Task<Result<IEnumerable<VehicleOutput>>> GetVehiclesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        if (string.IsNullOrWhiteSpace(personalIdentificationNumber))
        {
            _logger.LogError("Personal identification number cannot be null or empty.");
            throw new ArgumentException("Personal identification number cannot be null or empty.", nameof(personalIdentificationNumber));
        }
        var vehiclesCollection = await _vehicleRepo.GetVehiclesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
        if (vehiclesCollection == null || !vehiclesCollection.IsSuccess || !vehiclesCollection.Value.Any())
        {
            _logger.LogInformation($"No vehicles found for personal identification number {personalIdentificationNumber}.");
            return Result<IEnumerable<VehicleOutput>>.Success(Enumerable.Empty<VehicleOutput>());
        }
        _logger.LogInformation($"{vehiclesCollection.Value.Count()} vehicles retrieved for personal identification number {personalIdentificationNumber}.");
        return Result<IEnumerable<VehicleOutput>>.Success(vehiclesCollection.Value.Select(v => v.ToVehicleOutput()));

    }

    public async Task<Result<IEnumerable<VehicleOutput>>> GetVehiclesByPersonalIdsAsync(VehicleInsurance.Shared.DTOs.PersonIdentifiersRequest personIds)
    {
        if (!personIds.PersonalIdentificationNumbers.Any())
        {
            _logger.LogError("Personal identification numbers cannot be null or empty.");
            throw new ArgumentException("Personal identification numbers cannot be null or empty.", nameof(personIds));
        }
        var vehiclesByOwners = await _vehicleRepo.GetVehiclesByPersonalIdsAsync(personIds);
        if (vehiclesByOwners == null || !vehiclesByOwners.IsSuccess || !vehiclesByOwners.Value.Any())
        {
            _logger.LogInformation("No vehicles found for the provided personal identification numbers.");
            return Result<IEnumerable<VehicleOutput>>.Success(Enumerable.Empty<VehicleOutput>());
        }
        _logger.LogInformation($"{vehiclesByOwners.Value.Count()} vehicles retrieved for the provided personal identification numbers.");

        return Result<IEnumerable<VehicleOutput>>.Success(vehiclesByOwners.Value.Select(v => v.ToVehicleOutput()));
    }
}
