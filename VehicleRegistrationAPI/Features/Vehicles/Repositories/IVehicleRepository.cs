using VehicleInsurance.Shared;
using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Entities;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle> GetVehicleByIdAsync(Guid vehicleId);
    Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
    Task UpdateVehicleAsync(Guid vehicleId, Vehicle vehicle);
    Task DeleteVehicleAsync(Guid vehicleId);
    Task<bool> VehicleExistsAsync(string registrationNumber);
    Task<IEnumerable<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId);
    Task<Vehicle> GetVehicleByRegistrationNumberAsync(string registrationNumber);

    Task<IEnumerable<Vehicle>> GetVehiclesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
    Task<IEnumerable<Vehicle>> GetVehiclesByPersonalIdsAsync(VehicleInsurance.Shared.DTOs.PersonIdentifiersRequest personIds);
}
