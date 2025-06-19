using InsuranceAPI.Features.Insurance.DTOs;
using InsuranceAPI.Features.Insurance.Extensions;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.HttpClients;
using VehicleInsurance.Shared.DTOs;
using InsuranceAPI.Features.Insurance.Pricing;
using InsuranceAPI.Features.FeatureManagement.Services;
using InsuranceAPI.Features.FeatureManagement.DTOs;
namespace InsuranceAPI.Features.Insurance.Services;

public class InsuranceService(IInsuranceRepository _insuranceRepo,
ICarRegistrationAPIClient _apiClient,
ILogger<InsuranceService> _logger,
IFeatureManagementService _featureManagementService,
IPriceCalculatorFactory _priceCalculatorFactory) : IInsuranceService
{
    public async Task<Result<InsuranceOutput>> AddInsuranceAsync(InsuranceInput insuranceInput)
    {
        try
        {
            if (insuranceInput == null)
            {
                return Result<InsuranceOutput>.Failure("Insurance cannot be null.");
            }
            //Validate the insurance product ID if it exists
            var insuranceProduct = await _insuranceRepo.GetInsuranceProductByIdAsync(insuranceInput.InsuranceProductId);
            if (!insuranceProduct.IsSuccess || insuranceProduct.Value == null)
            {
                _logger.LogError("Insurance product not found for ID: {InsuranceProductId}", insuranceInput.InsuranceProductId);
                return Result<InsuranceOutput>.Failure(insuranceProduct.Error ?? "Insurance product not found for the provided ID.");
            }

            var customer = await _apiClient.GetCustomerByPersonalIdentificationNumberAsync(insuranceInput.PersonalIdentificationNumber);
            if (customer == null)
            {
                _logger.LogError("Customer not found for personal identification number: {PersonalIdentificationNumber}", insuranceInput.PersonalIdentificationNumber);
                return Result<InsuranceOutput>.Failure("Customer not found for the provided personal identification number.");
            }

            //Check if the insurace code is CAR and if so, fetch car details
            // from the Vehicle Registration API using the personal identification number
            if (insuranceProduct.Value.Code == "CAR")
            {
                var carDetails = await _apiClient.GetCarRegistrationAsync(insuranceInput.PersonalIdentificationNumber);
                if (carDetails == null)
                {
                    _logger.LogWarning("Car details not found for personal identification number: {PersonalIdentificationNumber}", insuranceInput.PersonalIdentificationNumber);
                    return Result<InsuranceOutput>.Failure("Car details not found for the provided personal identification number.");
                }
            }

            //Get feature toggles status
            var featureToggleNamesInput = new FeatureToggleNameInput(new List<string> { "ShowCarDetails", "ApplyDiscounts" });
            var featureTogglesResult = await _featureManagementService.GetFeatureTogglesByNamesAsync(featureToggleNamesInput);

            //Add the insurance to the repository
            var insuranceEntity = insuranceInput.ToEntity();
            var newInsurance = await _insuranceRepo.AddInsuranceAsync(insuranceEntity);
            if (!newInsurance.IsSuccess || newInsurance.Value == null)
            {
                _logger.LogError("Failed to add insurance: {Error}", newInsurance.Error);
                return Result<InsuranceOutput>.Failure(newInsurance.Error ?? "Failed to add insurance.");
            }
            newInsurance.Value.InsuranceProduct = insuranceProduct.Value;
            _logger.LogInformation("Insurance with ID {InsuranceId} added successfully.", newInsurance.Value.Id);

            return Result<InsuranceOutput>.Success(newInsurance.Value.ToOutput());
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding the insurance.");
            return Result<InsuranceOutput>.Failure("An error occurred while adding the insurance.");
        }
    }

    /// <summary>
    /// Retrieves all insurance records.
    /// This method fetches all insurances from the repository and maps them to their corresponding responses.
    /// </summary>
    /// <returns></returns>
    public async Task<Result<IEnumerable<InsuranceOutput>>> GetAllInsurancesAsync()
    {
        try
        {
            var insurances = await _insuranceRepo.GetAllInsurancesAsync();
            if (!insurances.IsSuccess)
            {
                _logger.LogError("Failed to retrieve insurances: {Error}", insurances.Error);
                return Result<IEnumerable<InsuranceOutput>>
                    .Failure(insurances.Error ?? "Failed to retrieve insurances.");
            }
            var insuranceValue = insurances?.Value ?? Enumerable.Empty<Entities.Insurance>();
            var carInsurances = insuranceValue
                            .Where(i => i.InsuranceProduct.Code == "CAR").ToList();

            if (!carInsurances.Any())
            {
                _logger.LogInformation("No car insurances. Fetching all insurances without car details.");
                return Result<IEnumerable<InsuranceOutput>>.Success(
                    insuranceValue.Select(i => i.ToOutput()));
            }

            //Make an Http call to Vehicle Registration API to get car details for Car insurances
            IEnumerable<CarDto> carDetails = await CallAPIForCarRegistrationsAsync(carInsurances);

            //Get car insurances with their corresponding car details
            var insurancesWithCars = GetCarInsurances(carDetails, carInsurances);

            _logger.LogInformation("Retrieved {Count} car insurances with details.", insurancesWithCars.Count());
            // Combine insurances without cars and those with cars
            var totalInsurances = insuranceValue
                                .Where(i => i.InsuranceProduct.Code != "CAR")
                                .Select(i => i.ToOutput())
                                .Concat(insurancesWithCars);

            return Result<IEnumerable<InsuranceOutput>>.Success(totalInsurances);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving insurances.");
            return Result<IEnumerable<InsuranceOutput>>.Failure($"An error occurred while retrieving insurances. {ex}");
        }

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

        var personIdentifiersRequest = new VehicleInsurance.Shared.DTOs.PersonIdentifiersRequest(personalIdentificationNumbers);
        _logger.LogInformation("Fetching car registrations for personal identification numbers: {PersonalIdentificationNumbers}",
            string.Join(", ", personIdentifiersRequest.PersonalIdentificationNumbers));

        // Call the API client to get car registrations by personal identification numbers
        return await _apiClient.GetCarRegistrationsByPersonIdsAsync(personIdentifiersRequest);
    }

    /// <summary>
    /// This method maps car insurances to their corresponding car details.
    /// </summary>
    private IEnumerable<InsuranceOutput> GetCarInsurances(IEnumerable<CarDto> carDetails,
        List<Entities.Insurance> carInsurances)
    {
        //Create a dictionary for quick lookup of car details by personal identification number
        var carDetailsDict = carDetails
                        .GroupBy(cd => cd.Owner.PersonalIdentificationNumber)
                        .ToDictionary(g => g.Key, g => g.ToList());

        var insurancesWithCars = carInsurances.Select(ci => ci.ToOutput());
        return insurancesWithCars.Select(insurance =>
        {
            // For each insurance, try to get the car detail from the dictionary based 
            // on the personal identification number
            carDetailsDict.TryGetValue(insurance.PersonalIdentificationNumber, out var carDetail);
            return insurance with { Car = carDetail?.FirstOrDefault(cd => cd.Owner.PersonalIdentificationNumber == insurance.PersonalIdentificationNumber) };
        });
    }

    /// <summary>
    /// Retrieves an insurance record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the insurance record.</param>
    public async Task<Result<Entities.Insurance>> GetInsuranceByIdAsync(Guid id)
    {
        try
        {

            var insurance = await _insuranceRepo.GetInsuranceByIdAsync(id);
            if (!insurance.IsSuccess)
            {
                _logger.LogError("Failed to retrieve insurance by ID {Id}: {Error}", id, insurance.Error);
                return Result<Entities.Insurance>.Failure(insurance.Error ?? "Failed to retrieve insurance.");
            }
            _logger.LogInformation("Successfully retrieved insurance by ID {Id}", id);
            return Result<Entities.Insurance>.Success(insurance.Value);

        }
        catch (System.Exception)
        {
            _logger.LogError("An error occurred while retrieving the insurance by ID {Id}", id);
            return Result<Entities.Insurance>.Failure("An error occurred while retrieving the insurance.");
        }
    }

    /// <summary>
    /// Retrieves all insurance records associated with a specific personal identification number.
    /// </summary>
    /// <param name="personalIdentificationNumber"></param>
    /// <returns></returns>
    public async Task<Result<IEnumerable<InsuranceOutput>>> GetInsurancesByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        try
        {

            var customer = await _apiClient.GetCustomerByPersonalIdentificationNumberAsync(personalIdentificationNumber);
            if (customer == null)
            {
                _logger.LogError("Customer not found for personal identification number: {PersonalIdentificationNumber}", personalIdentificationNumber);
                return Result<IEnumerable<InsuranceOutput>>.Failure("Customer not found for the provided personal identification number.");
            }

            var insurancesResult = await _insuranceRepo.GetInsurancesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
            if (!insurancesResult.IsSuccess)
            {
                return Result<IEnumerable<InsuranceOutput>>.Failure(insurancesResult.Error ?? "Failed to retrieve insurances.");
            }

            var insurances = insurancesResult.Value ?? Enumerable.Empty<Entities.Insurance>();
            var carInsurances = insurances.Where(i => i.InsuranceProduct.Code == "CAR").ToList();
            var otherInsurances = insurances.Where(i => i.InsuranceProduct.Code != "CAR").ToList();

            var insuranceResponses = new List<InsuranceOutput>();

            if (carInsurances.Any())
                insuranceResponses.AddRange(await GetCarInsuranceResponsesAsync(carInsurances, personalIdentificationNumber));

            insuranceResponses.AddRange(GetOtherInsuranceResponses(otherInsurances, personalIdentificationNumber));

            return Result<IEnumerable<InsuranceOutput>>.Success(insuranceResponses);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving insurances for personal identification number: {PersonalIdentificationNumber}", personalIdentificationNumber);
            return Result<IEnumerable<InsuranceOutput>>.Failure("An error occurred while retrieving insurances.");
        }
    }

    private async Task<IEnumerable<InsuranceOutput>> GetCarInsuranceResponsesAsync(
        List<Entities.Insurance> carInsurances, string personalIdentificationNumber)
    {
        var responses = new List<InsuranceOutput>();
        var carDetails = await _apiClient.GetCarRegistrationAsync(personalIdentificationNumber);

        if (carDetails == null)
        {
            _logger.LogWarning("No car details found for personal identification number: {PersonalIdentificationNumber}", personalIdentificationNumber);
            return responses;
        }

        foreach (var insurance in carInsurances)
        {
            var carDetail = carDetails.FirstOrDefault(cd =>
                cd.Owner.PersonalIdentificationNumber == insurance.PersonalIdentificationNumber &&
                cd.RegistrationNumber == insurance.InsuredItem);

            var insuranceOutput = insurance.ToOutput() with { Car = carDetail };
            responses.Add(insuranceOutput);
        }

        _logger.LogInformation("Retrieved {Count} car insurances with details for personal identification number: {PersonalIdentificationNumber}",
            carInsurances.Count, personalIdentificationNumber);

        return responses;
    }

    private IEnumerable<InsuranceOutput> GetOtherInsuranceResponses(
        List<Entities.Insurance> otherInsurances, string personalIdentificationNumber)
    {
        var responses = otherInsurances.Select(insurance => insurance.ToOutput()).ToList();

        _logger.LogInformation("Retrieved {Count} other insurances for personal identification number: {PersonalIdentificationNumber}",
            responses.Count, personalIdentificationNumber);

        return responses;
    }
}
