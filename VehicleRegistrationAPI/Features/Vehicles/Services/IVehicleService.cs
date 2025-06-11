using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Services;

public interface IVehicleService
{
    Task<VehicleOutput> GetVehicleByIdAsync(Guid vehicleId);
    Task<IEnumerable<VehicleOutput>> GetAllVehiclesAsync();
    Task<VehicleOutput> AddVehicleAsync(VehicleInput vehicleInput);
    Task<VehicleOutput> UpdateVehicleAsync(Guid vehicleId, VehicleInput vehicleInput);
    Task DeleteVehicleAsync(Guid vehicleId);
    Task<bool> VehicleExistsAsync(string registrationNumber);
    Task<IEnumerable<VehicleOutput>> GetVehiclesByCustomerIdAsync(Guid customerId);
    Task<VehicleOutput> GetVehicleByRegistrationNumberAsync(string registrationNumber);
}
