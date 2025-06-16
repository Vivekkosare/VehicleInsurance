using VehicleInsurance.Shared;
using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Entities;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Repositories;

public interface IVehicleRepository
{
    Task<Result<Vehicle>> GetVehicleByIdAsync(Guid vehicleId);
    Task<Result<IEnumerable<Vehicle>>> GetAllVehiclesAsync();
    Task<Result<Vehicle>> AddVehicleAsync(Vehicle vehicle);
    Task<Result<bool>> UpdateVehicleAsync(Guid vehicleId, Vehicle vehicle);
    Task<Result<bool>> DeleteVehicleAsync(Guid vehicleId);
    Task<Result<bool>> VehicleExistsAsync(string registrationNumber);
    Task<Result<IEnumerable<Vehicle>>> GetVehiclesByCustomerIdAsync(Guid customerId);
    Task<Result<Vehicle>> GetVehicleByRegistrationNumberAsync(string registrationNumber);

    Task<Result<IEnumerable<Vehicle>>> GetVehiclesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
    Task<Result<IEnumerable<Vehicle>>> GetVehiclesByPersonalIdsAsync(VehicleInsurance.Shared.DTOs.PersonIdentifiersRequest personIds);
    Task<Result<bool>> SetVehicleExitsCacheAsync(string registrationNumber, bool exists);
}
