namespace InsuranceAPI.Features.Insurance.Repositories;

using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Entities;
public interface IInsuranceRepository
{
    Task<IEnumerable<Insurance>> GetAllInsurancesAsync();
    Task<Insurance> GetInsuranceByIdAsync(Guid id);
    Task<Insurance> AddInsuranceAsync(Insurance insurance);
     Task<ICollection<Insurance>>  GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
    
}
