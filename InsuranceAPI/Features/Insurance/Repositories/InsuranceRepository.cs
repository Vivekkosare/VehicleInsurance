using InsuranceAPI.Data;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Features.Insurance.Entities;
using InsuranceAPI.HttpClients;
using VehicleInsurance.Shared;

namespace InsuranceAPI.Features.Insurance.Repositories;

public class InsuranceRepository(InsuranceDbContext _dbContext) : IInsuranceRepository
{
    public async Task<Result<Entities.Insurance>> AddInsuranceAsync(Entities.Insurance insurance)
    {
        var existingInsurance = await _dbContext.Insurances.Include(i => i.InsuranceProduct)
                                      .FirstOrDefaultAsync(i => i.PersonalIdentificationNumber == insurance.PersonalIdentificationNumber &&
                                    i.InsuranceProductId == insurance.InsuranceProductId);
        if (existingInsurance != null)
        {
            return Result<Entities.Insurance>.Failure("Insurance with the same personal identification number and insurance product already exists.");
        }

        var newInsurance = await _dbContext.Insurances.AddAsync(insurance);
        await _dbContext.SaveChangesAsync();
        return Result<Entities.Insurance>.Success(newInsurance.Entity);
    }

    public async Task<Result<IEnumerable<Entities.Insurance>>> GetAllInsurancesAsync()
    {
        var insurances = await _dbContext.Insurances
                        .Include(i => i.InsuranceProduct).ToListAsync();

        return Result<IEnumerable<Entities.Insurance>>.Success(insurances ?? new List<Entities.Insurance>());
    }

    public async Task<Result<Entities.Insurance>> GetInsuranceByIdAsync(Guid id)
    {
        var insurance = await _dbContext.Insurances.FirstOrDefaultAsync(i => i.Id == id);
        if (insurance == null)
        {
            return Result<Entities.Insurance>.Failure($"Insurance with ID {id} not found.");
        }
        return Result<Entities.Insurance>.Success(insurance);
    }

    public async Task<Result<IEnumerable<Entities.Insurance>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        var insurances = await _dbContext.Insurances.Include(i => i.InsuranceProduct)
            .Where(i => i.PersonalIdentificationNumber == personalIdentificationNumber)
            .ToListAsync();
        if (insurances == null || !insurances.Any())
        {
            return Result<IEnumerable<Entities.Insurance>>.Failure($"No insurances found for personal identification number {personalIdentificationNumber}.");
        }

        return Result<IEnumerable<Entities.Insurance>>.Success(insurances);
    }
}
