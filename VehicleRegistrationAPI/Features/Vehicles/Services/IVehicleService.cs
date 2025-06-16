using VehicleInsurance.Shared;
using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Services;

public interface IVehicleService
{
    Task<Result<VehicleOutput>> GetVehicleByIdAsync(Guid vehicleId);
    Task<Result<IEnumerable<VehicleOutput>>> GetAllVehiclesAsync();
    Task<Result<VehicleOutput>> AddVehicleAsync(VehicleInput vehicleInput);
    Task<Result<VehicleOutput>> UpdateVehicleAsync(Guid vehicleId, VehicleInput vehicleInput);
    Task<Result<bool>> DeleteVehicleAsync(Guid vehicleId);
    Task<Result<bool>> VehicleExistsAsync(string registrationNumber);
    Task<Result<IEnumerable<VehicleOutput>>> GetVehiclesByCustomerIdAsync(Guid customerId);
    Task<Result<VehicleOutput>> GetVehicleByRegistrationNumberAsync(string registrationNumber);
    Task<Result<IEnumerable<VehicleOutput>>> GetVehiclesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
    Task<Result<IEnumerable<VehicleOutput>>> GetVehiclesByPersonalIdsAsync(PersonIdentifiersRequest personIds);

}