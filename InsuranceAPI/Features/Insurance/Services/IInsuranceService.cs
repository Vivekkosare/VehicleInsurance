using InsuranceAPI.DTOs;

namespace InsuranceAPI.Features.Insurance.Services;

public interface IInsuranceService
{
    Task<IEnumerable<Entities.Insurance>> GetAllInsurancesAsync();
    Task<Entities.Insurance> GetInsuranceByIdAsync(Guid id);
    Task<Entities.Insurance> AddInsuranceAsync(Entities.Insurance insurance);
    Task<IEnumerable<InsuranceResponse>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
}
