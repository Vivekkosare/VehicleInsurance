using InsuranceAPI.Data;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Features.Insurance.Entities;
using VehicleInsurance.Shared.DTOs;
using VehicleInsurance.Shared.Services;

namespace InsuranceAPI.Features.Insurance.Repositories;

public class InsuranceRepository(InsuranceDbContext _dbContext,
ILogger<InsuranceRepository> _logger,
ICacheService _cache,
IConfiguration _config) : IInsuranceRepository
{
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(_config.GetValue<int>("TimeoutInMinutes"));
    public async Task<Result<Entities.Insurance>> AddInsuranceAsync(Entities.Insurance insurance)
    {
        var existingInsurance = await GetExistingInsuranceAsync(insurance);
        if (existingInsurance != null)
        {
            _logger.LogWarning("Insurance with the same personal identification number, insured item and insurance product already exists.");
            return Result<Entities.Insurance>.Failure("Insurance with the same personal identification number, insuread item and insurance product already exists.");
        }

        var newInsurance = await _dbContext.Insurances.AddAsync(insurance);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Insurance with ID {InsuranceId} created successfully.", newInsurance.Entity.Id);
        return Result<Entities.Insurance>.Success(newInsurance.Entity);
    }

    private async Task<Entities.Insurance?> GetExistingInsuranceAsync(Entities.Insurance insurance)
    {
        string insuraceCacheKey = $"{insurance.PersonalIdentificationNumber}_{insurance.InsuredItem}_{insurance.InsuranceProductId}";
        var existingInsuranceFromCache = await _cache.GetAsync<Entities.Insurance>(insuraceCacheKey);
        if (existingInsuranceFromCache.IsSuccess)
        {
            _logger.LogInformation("Found existing insurance in cache for key: {CacheKey}", insuraceCacheKey);
            return existingInsuranceFromCache.Value;
        }
        var existingInsurance = await _dbContext.Insurances.Include(i => i.InsuranceProduct)
                                     .FirstOrDefaultAsync(i => i.PersonalIdentificationNumber == insurance.PersonalIdentificationNumber &&
                                     i.InsuredItem == insurance.InsuredItem &&
                                     i.InsuranceProductId == insurance.InsuranceProductId);
        await _cache.SetAsync(insuraceCacheKey, existingInsurance, _cacheTimeout);
        return existingInsurance;
    }

    public async Task<Result<IEnumerable<Entities.Insurance>>> GetAllInsurancesAsync()
    {
        var insurances = await _dbContext.Insurances
                        .Include(i => i.InsuranceProduct).ToListAsync();
        _logger.LogInformation("Retrieved {Count} insurances from the database.", insurances.Count);
        return Result<IEnumerable<Entities.Insurance>>.Success(insurances ?? new List<Entities.Insurance>());
    }

    public async Task<Result<Entities.Insurance>> GetInsuranceByIdAsync(Guid id)
    {
        // Check if the insurance is in cache
        var insuranceCacheKey = $"Insurance_{id}";
        var cachedInsuraceById = await _cache.GetAsync<Entities.Insurance>(insuranceCacheKey);

        if (cachedInsuraceById.IsSuccess && cachedInsuraceById.Value != null)
        {
            _logger.LogInformation("Found insurance in cache for key: {CacheKey}", insuranceCacheKey);
            return Result<Entities.Insurance>.Success(cachedInsuraceById.Value);
        }

        // If not in cache, retrieve from the database
        var insurance = await _dbContext.Insurances.FirstOrDefaultAsync(i => i.Id == id);
        if (insurance == null)
        {
            _logger.LogWarning("Insurance with ID {InsuranceId} not found.", id);
            return Result<Entities.Insurance>.Failure($"Insurance with ID {id} not found.");
        }
        _logger.LogInformation("Insurance with ID {InsuranceId} retrieved successfully.", id);

        // Cache the insurance for future requests
        await _cache.SetAsync(insuranceCacheKey, insurance, _cacheTimeout);

        return Result<Entities.Insurance>.Success(insurance);
    }

    public async Task<Result<InsuranceProduct>> GetInsuranceProductByIdAsync(Guid id)
    {
        var insuranceProductCacheKey = $"InsuranceProduct_{id}";
        var cachedInsuranceProduct = await _cache.GetAsync<InsuranceProduct>(insuranceProductCacheKey);

        if (cachedInsuranceProduct.IsSuccess && cachedInsuranceProduct.Value != null)
        {
            _logger.LogInformation("Found insurance product in cache for key: {CacheKey}", insuranceProductCacheKey);
            return Result<InsuranceProduct>.Success(cachedInsuranceProduct.Value);
        }
        // If not in cache, retrieve from the database
        var insuranceProduct = await _dbContext.InsuranceProducts.FirstOrDefaultAsync(i => i.Id == id);
        if (insuranceProduct == null)
        {
            _logger.LogWarning("Insurance product with ID {InsuranceProductId} not found.", id);
            return Result<InsuranceProduct>.Failure($"Insurance product with ID {id} not found.");
        }
        _logger.LogInformation("Insurance product with ID {InsuranceProductId} retrieved successfully.", id);

        // Cache the insurance product for future requests
        await _cache.SetAsync(insuranceProductCacheKey, insuranceProduct, _cacheTimeout);

        return Result<InsuranceProduct>.Success(insuranceProduct);
    }

    public async Task<Result<IEnumerable<Entities.Insurance>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        var cacheKey = $"Insurances_{personalIdentificationNumber}";

        //Check if the insurances are in cache
        var cachedInsurances = await _cache.GetAsync<IEnumerable<Entities.Insurance>>(cacheKey);
        if (cachedInsurances.IsSuccess && cachedInsurances.Value != null)
        {
            _logger.LogInformation("Found insurances in cache for key: {CacheKey}", cacheKey);
            return Result<IEnumerable<Entities.Insurance>>.Success(cachedInsurances.Value);
        }

        // If not in cache, retrieve from the database
        var insurances = await _dbContext.Insurances.Include(i => i.InsuranceProduct)
            .Where(i => i.PersonalIdentificationNumber == personalIdentificationNumber)
            .ToListAsync();
        if (insurances == null || !insurances.Any())
        {
            _logger.LogWarning("No insurances found for personal identification number {PersonalIdentificationNumber}.", personalIdentificationNumber);
            return Result<IEnumerable<Entities.Insurance>>.Failure($"No insurances found for personal identification number {personalIdentificationNumber}.");
        }
        _logger.LogInformation("Retrieved {Count} insurances for personal identification number {PersonalIdentificationNumber}.", insurances.Count, personalIdentificationNumber);

        // Cache the insurances for future requests
        await _cache.SetAsync(cacheKey, insurances, _cacheTimeout);

        return Result<IEnumerable<Entities.Insurance>>.Success(insurances);
    }
}
