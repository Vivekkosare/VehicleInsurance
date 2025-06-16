using InsuranceAPI.DTOs;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.Insurance.Services;

public interface IInsuranceService
{
    Task<Result<IEnumerable<InsuranceOutput>>> GetAllInsurancesAsync();
    Task<Result<Entities.Insurance>> GetInsuranceByIdAsync(Guid id);
    Task<Result<InsuranceOutput>> AddInsuranceAsync(InsuranceInput insuranceInput);
    Task<Result<IEnumerable<InsuranceOutput>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
}
