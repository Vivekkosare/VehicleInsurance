using VehicleRegistrationAPI.Features.Vehicles.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.Extensions;
using VehicleRegistrationAPI.Features.Vehicles.Repositories;

namespace VehicleRegistrationAPI.Features.Vehicles.Services;

public class VehicleService(IVehicleRepository _vehicleRepo) : IVehicleService
{
    public async Task<VehicleOutput> AddVehicleAsync(VehicleInput vehicleInput)
    {
        if (vehicleInput == null)
        {
            throw new ArgumentNullException(nameof(vehicleInput), "Vehicle input cannot be null.");
        }
        var vehicle = vehicleInput.ToVehicleEntity();
        bool vehicleExists = await VehicleExistsAsync(vehicle.RegistrationNumber);
        if (vehicleExists)
        {
            throw new InvalidOperationException($"Vehicle with registration number {vehicle.RegistrationNumber} already exists.");
        }
        var addedVehicle = await _vehicleRepo.AddVehicleAsync(vehicle);
        return addedVehicle.ToVehicleOutput();
    }

    public Task DeleteVehicleAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }
        return _vehicleRepo.DeleteVehicleAsync(vehicleId);
    }

    public async Task<IEnumerable<VehicleOutput>> GetAllVehiclesAsync()
    {
        var vehicles = await _vehicleRepo.GetAllVehiclesAsync();
        return vehicles.Select(v => v.ToVehicleOutput());
    }

    public async Task<VehicleOutput> GetVehicleByIdAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }
        var vehicle = await _vehicleRepo.GetVehicleByIdAsync(vehicleId);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");
        }
        return vehicle.ToVehicleOutput();
    }

    public async Task<VehicleOutput> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("Registration number cannot be null or empty.", nameof(registrationNumber));
        }
        var vehicle = await _vehicleRepo.GetVehicleByRegistrationNumberAsync(registrationNumber);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with registration number {registrationNumber} not found.");
        }
        return vehicle.ToVehicleOutput();
    }

    public async Task<IEnumerable<VehicleOutput>> GetVehiclesByCustomerIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }
        var vehicle = await _vehicleRepo.GetVehiclesByCustomerIdAsync(customerId);
        if (vehicle == null || !vehicle.Any())
        {
            throw new KeyNotFoundException($"No vehicles found for customer with ID {customerId}.");
        }
        return vehicle.Select(v => v.ToVehicleOutput());
    }

    public async Task<VehicleOutput> UpdateVehicleAsync(Guid vehicleId, VehicleInput vehicleInput)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }
        if (vehicleInput == null)
        {
            throw new ArgumentNullException(nameof(vehicleInput), "Vehicle input cannot be null.");
        }
        var vehicle = vehicleInput.ToVehicleEntity();
        await _vehicleRepo.UpdateVehicleAsync(vehicleId, vehicle);

        var updatedVehicle = await _vehicleRepo.GetVehicleByIdAsync(vehicleId);
        if (updatedVehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found after update.");
        }
        return updatedVehicle.ToVehicleOutput();
    }

    public async Task<bool> VehicleExistsAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("Registration number cannot be null or empty.", nameof(registrationNumber));
        }
        return await _vehicleRepo.VehicleExistsAsync(registrationNumber);
    }
}
