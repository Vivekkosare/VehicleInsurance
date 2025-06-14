namespace InsuranceAPI.Features.Insurance.Repositories;

using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Entities;
using VehicleInsurance.Shared;

public interface IInsuranceRepository
{
    Task<Result<IEnumerable<Insurance>>> GetAllInsurancesAsync();
    Task<Result<Insurance>> GetInsuranceByIdAsync(Guid id);
    Task<Result<Insurance>> AddInsuranceAsync(Insurance insurance);
    Task<Result<IEnumerable<Insurance>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);

}
