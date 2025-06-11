using Microsoft.EntityFrameworkCore;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Features.Vehicles.Repositories;

public class VehicleRepository(VehicleRegistrationDbContext dbContext) : IVehicleRepository
{
    public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            throw new ArgumentNullException(nameof(vehicle));
        }
        bool vehicleExists = await VehicleExistsAsync(vehicle.RegistrationNumber);
        if (vehicleExists)
        {
            throw new InvalidOperationException($"Vehicle with registration number {vehicle.RegistrationNumber} already exists.");
        }
        var newVehicle = await dbContext.Vehicles.AddAsync(vehicle);
        await dbContext.SaveChangesAsync();
        return newVehicle.Entity;
    }

    public async Task DeleteVehicleAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }

        var vehicle = dbContext.Vehicles.Find(vehicleId);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");
        }

        dbContext.Vehicles.Remove(vehicle);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
    {
        var vehicles = await dbContext.Vehicles.ToListAsync();
        return vehicles ?? new List<Vehicle>();
    }

    public async Task<Vehicle> GetVehicleByIdAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }

        var vehicle = await dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");
        }

        return vehicle;
    }

    public async Task<Vehicle> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("Registration number cannot be null or empty.", nameof(registrationNumber));
        }

        var vehicle = await dbContext.Vehicles
                        .FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with registration number {registrationNumber} not found.");
        }
        return vehicle;
    }

    public async Task<IEnumerable<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }

        var vehicles = await dbContext.Vehicles
            .Where(v => v.OwnerId == customerId)
            .ToListAsync();
        return vehicles ?? new List<Vehicle>();
    }

    public async Task UpdateVehicleAsync(Guid vehicleId, Vehicle vehicle)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle ID cannot be empty.", nameof(vehicleId));
        }
        if (vehicle == null)
        {
            throw new ArgumentNullException(nameof(vehicle));
        }

        var existingVehicle = await dbContext.Vehicles.FindAsync(vehicleId);
        if (existingVehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");
        }
        await dbContext.Entry(existingVehicle).ReloadAsync();
        dbContext.Entry(existingVehicle).CurrentValues.SetValues(vehicle);
        // If you need to update navigation properties, you can do so here

        dbContext.Vehicles.Update(existingVehicle);
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> VehicleExistsAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("Registration number cannot be null or empty.", nameof(registrationNumber));
        }

        return dbContext.Vehicles.AnyAsync(v => v.RegistrationNumber == registrationNumber);
    }
}
