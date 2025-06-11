using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Extensions;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.HttpClients;
namespace InsuranceAPI.Features.Insurance.Services;

public class InsuranceService(IInsuranceRepository _insuranceRepo,
CarRegistrationAPIClient _apiClient) : IInsuranceService
{
    public async Task<Entities.Insurance> AddInsuranceAsync(Entities.Insurance insurance)
    {
        if (insurance == null)
        {
            throw new ArgumentNullException(nameof(insurance), "Insurance cannot be null.");
        }
        var carDetails = await _apiClient.GetCarRegistrationAsync(insurance.PersonalIdentificationNumber);
        if (carDetails == null)
        {
            throw new InvalidOperationException("Car details not found for the provided personal identification number.");
        }

        var newInsurance = await _insuranceRepo.AddInsuranceAsync(insurance);
        return newInsurance;
    }

    public async Task<IEnumerable<Entities.Insurance>> GetAllInsurancesAsync()
    {
        return await _insuranceRepo.GetAllInsurancesAsync();
    }

    public async Task<Entities.Insurance> GetInsuranceByIdAsync(Guid id)
    {
         return await _insuranceRepo.GetInsuranceByIdAsync(id);
    }

    public async Task<IEnumerable<InsuranceResponse>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        List<InsuranceResponse> insuranceResponses = new();
        var insurances = await _insuranceRepo
                    .GetInsurancesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
        
        foreach (var insurance in insurances)
        {
            var insuranceResponse = insurance.ToResponse();
            if (insurance.InsuranceProduct.Code == "CAR")
            {
                var carDetails = await _apiClient.GetCarRegistrationAsync(insurance.PersonalIdentificationNumber);
                insuranceResponse = insuranceResponse with { Car = carDetails };
            }
            insuranceResponses.Add(insuranceResponse);
        }

        return insuranceResponses;
    }
}
