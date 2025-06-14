using System.Collections.Generic;
using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Extensions;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.HttpClients;
using VehicleInsurance.Shared;
namespace InsuranceAPI.Features.Insurance.Services;

public class InsuranceService(IInsuranceRepository _insuranceRepo,
CarRegistrationAPIClient _apiClient) : IInsuranceService
{
    public async Task<Result<InsuranceResponse>> AddInsuranceAsync(Entities.Insurance insurance)
    {
        try
        {
            if (insurance == null)
            {
                return Result<InsuranceResponse>.Failure("Insurance cannot be null.");
            }
            var carDetails = await _apiClient.GetCarRegistrationAsync(insurance.PersonalIdentificationNumber);
            if (carDetails == null)
            {
                return Result<InsuranceResponse>.Failure("Car details not found for the provided personal identification number.");
            }

            var newInsurance = await _insuranceRepo.AddInsuranceAsync(insurance);
            if (!newInsurance.IsSuccess || newInsurance.Value == null)
            {
                return Result<InsuranceResponse>.Failure(newInsurance.Error ?? "Failed to add insurance.");
            }
            return Result<InsuranceResponse>.Success(newInsurance.Value.ToResponse() with
            {
                Car = carDetails.FirstOrDefault()
            });
        }
        catch (System.Exception)
        {
            return Result<InsuranceResponse>.Failure("An error occurred while adding the insurance.");
        }
    }

    /// <summary>
    /// Retrieves all insurance records.
    /// This method fetches all insurances from the repository and maps them to their corresponding responses.
    /// </summary>
    /// <returns></returns>
    public async Task<Result<IEnumerable<InsuranceResponse>>> GetAllInsurancesAsync()
    {
        var insurances = await _insuranceRepo.GetAllInsurancesAsync();
        if (!insurances.IsSuccess)
        {
            return Result<IEnumerable<InsuranceResponse>>
                .Failure(insurances.Error ?? "Failed to retrieve insurances.");
        }
        var insuranceValue = insurances?.Value ?? Enumerable.Empty<Entities.Insurance>();
        var carInsurances = insuranceValue
                        .Where(i => i.InsuranceProduct.Code == "CAR").ToList();

        if (!carInsurances.Any())
        {
            return Result<IEnumerable<InsuranceResponse>>.Success(insuranceValue
                .Select(i => i.ToResponse()));
        }

        //Make an Http call to Vehicle Registration API to get car details for Car insurances
        IEnumerable<CarDto> carDetails = await CallAPIForCarRegistrationsAsync(carInsurances);

        //Get car insurances with their corresponding car details
        var insurancesWithCars = GetCarInsurances(carDetails, carInsurances);

        // Combine insurances without cars and those with cars
        var totalInsurances = insuranceValue
                            .Where(i => i.InsuranceProduct.Code != "CAR")
                            .Select(i => i.ToResponse())
                            .Concat(insurancesWithCars);

        return Result<IEnumerable<InsuranceResponse>>.Success(totalInsurances);
    }

    /// <summary>
    /// If there are car insurances, fetch car details for all unique personal identification numbers
    /// associated with those insurances 
    /// This method makes an HTTP call to the Vehicle Registration API to retrieve car details
    /// for the given personal identification numbers.
    /// </summary>
    /// <param name="carInsurances"></param>
    /// <returns></returns>
    private async Task<IEnumerable<CarDto>> CallAPIForCarRegistrationsAsync(IEnumerable<Entities.Insurance> carInsurances)
    {
        var personalIdentificationNumbers = carInsurances
            .Where(i => i.InsuranceProduct.Code == "CAR")
            .Select(i => i.PersonalIdentificationNumber)
            .Distinct()
            .ToArray();

        var personIdentifiersRequest = new PersonIdentifiersRequest(personalIdentificationNumbers);
        return await _apiClient.GetCarRegistrationsByPersonIdsAsync(personIdentifiersRequest);
    }

    /// <summary>
    /// This method maps car insurances to their corresponding car details.
    /// </summary>
    private IEnumerable<InsuranceResponse> GetCarInsurances(IEnumerable<CarDto> carDetails,
        List<Entities.Insurance> carInsurances)
    {
        //Create a dictionary for quick lookup of car details by personal identification number
        var carDetailsDict = carDetails.ToDictionary(cd => cd.Owner.PersonalIdentificationNumber, cd => cd);

        var insurancesWithCars = carInsurances.Select(ci => ci.ToResponse());
        return insurancesWithCars.Select(i =>
        {
            // For each insurance, try to get the car detail from the dictionary based 
            // on the personal identification number
            carDetailsDict.TryGetValue(i.PersonalIdentificationNumber, out var carDetail);
            return i with { Car = carDetail };
        });
    }

    /// <summary>
    /// Retrieves an insurance record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the insurance record.</param>
    public async Task<Result<Entities.Insurance>> GetInsuranceByIdAsync(Guid id)
    {
        return await _insuranceRepo.GetInsuranceByIdAsync(id);
    }

    /// <summary>
    /// Retrieves all insurance records associated with a specific personal identification number.
    /// </summary>
    /// <param name="personalIdentificationNumber"></param>
    /// <returns></returns>
    public async Task<Result<IEnumerable<InsuranceResponse>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        List<InsuranceResponse> insuranceResponses = new();
        var insurances = await _insuranceRepo
                    .GetInsurancesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
        if (!insurances.IsSuccess)
        {
            return Result<IEnumerable<InsuranceResponse>>.Failure(insurances.Error ?? "Failed to retrieve insurances.");
        }

        if (insurances.Value != null)
        {
            foreach (var insurance in insurances.Value)
            {
                var carDetails = insurance.InsuranceProduct.Code == "CAR"
                    ? await _apiClient.GetCarRegistrationAsync(insurance.PersonalIdentificationNumber)
                    : null;

                var insuranceResponse = insurance.ToResponse() with { Car = carDetails?.FirstOrDefault() };
                insuranceResponses.Add(insuranceResponse);
            }
        }

        return Result<IEnumerable<InsuranceResponse>>.Success(insuranceResponses);
    }
}
