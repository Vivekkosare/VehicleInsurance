using InsuranceAPI.DTOs;
using VehicleInsurance.Shared;

namespace InsuranceAPI.Features.Insurance.Services;

public interface IInsuranceService
{
    Task<Result<IEnumerable<InsuranceResponse>>> GetAllInsurancesAsync();
    Task<Result<Entities.Insurance>> GetInsuranceByIdAsync(Guid id);
    Task<Result<InsuranceResponse>> AddInsuranceAsync(Entities.Insurance insurance);
    Task<Result<IEnumerable<InsuranceResponse>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
}
