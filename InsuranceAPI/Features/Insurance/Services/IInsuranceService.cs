using InsuranceAPI.Features.Insurance.DTOs;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.Insurance.Services;

public interface IInsuranceService
{
    Task<Result<IEnumerable<InsuranceOutput>>> GetAllInsurancesAsync();
    Task<Result<InsuranceOutput>> GetInsuranceByIdAsync(Guid id);
    Task<Result<InsuranceOutput>> AddInsuranceAsync(InsuranceInput insuranceInput);
    Task<Result<IEnumerable<InsuranceOutput>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
}
