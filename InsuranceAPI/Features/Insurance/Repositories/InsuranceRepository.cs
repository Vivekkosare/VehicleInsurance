using InsuranceAPI.Data;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Features.Insurance.Entities;
using InsuranceAPI.HttpClients;

namespace InsuranceAPI.Features.Insurance.Repositories;

public class InsuranceRepository(InsuranceDbContext _dbContext) : IInsuranceRepository
{
    public async Task<Entities.Insurance> AddInsuranceAsync(Entities.Insurance insurance)
    {
        var existingInsurance = await _dbContext.Insurances.Include(i => i.InsuranceProduct)
                                      .FirstOrDefaultAsync(i => i.PersonalIdentificationNumber == insurance.PersonalIdentificationNumber &&
                                    i.InsuranceProductId == insurance.InsuranceProductId);
        if (existingInsurance != null)
        {
            throw new InvalidOperationException("Insurance with the same personal identification number and insurance product already exists.");
        }

        var newInsurance = await _dbContext.Insurances.AddAsync(insurance);
        await _dbContext.SaveChangesAsync();
        return newInsurance.Entity;
    }

    public async Task<IEnumerable<Entities.Insurance>> GetAllInsurancesAsync()
    {
        var insurances = await _dbContext.Insurances.ToListAsync();

        return insurances ?? new List<Entities.Insurance>();
    }

    public async Task<Entities.Insurance> GetInsuranceByIdAsync(Guid id)
    {
        var insurance = await _dbContext.Insurances.FirstOrDefaultAsync(i => i.Id == id);
        if (insurance == null)
        {
            throw new KeyNotFoundException($"Insurance with ID {id} not found.");
        }
        return insurance;
    }

    public async Task<ICollection<Entities.Insurance>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        var insurances = await _dbContext.Insurances.Include(i => i.InsuranceProduct)
            .Where(i => i.PersonalIdentificationNumber == personalIdentificationNumber)
            .ToListAsync();
        
        return insurances ?? throw new KeyNotFoundException($"No insurances found for personal identification number {personalIdentificationNumber}.");
    }
}
