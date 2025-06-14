using System.Net.Http.Headers;
using InsuranceAPI.DTOs;
using VehicleInsurance.Shared;

namespace InsuranceAPI.HttpClients;

public class CarRegistrationAPIClient
{
    private readonly HttpClient _httpClient;

    public CarRegistrationAPIClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    public async Task<IEnumerable<CarDto>> GetCarRegistrationAsync(string personalIdentificationNumber)
    {
        if (string.IsNullOrEmpty(personalIdentificationNumber))
        {
            throw new ArgumentException("Personal identification number cannot be null or empty.", nameof(personalIdentificationNumber));
        }

        // Use the correct path relative to the base address
        var response = await _httpClient.GetAsync($"/api/v1/vehicles/personal/{personalIdentificationNumber}");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error fetching car registration: {response.ReasonPhrase}");
        }
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(data))
        {
            throw new InvalidOperationException("No car registration data found for the provided personal identification number.");
        }
        var car = System.Text.Json.JsonSerializer.Deserialize<List<CarDto>>(data);
        if (car == null)
        {
            throw new InvalidOperationException("Deserialized car object is null.");
        }
        return car;
    }

    public async Task<IEnumerable<CarDto>> GetCarRegistrationsByPersonIdsAsync(PersonIdentifiersRequest personIdsRequest)
    {
        if (personIdsRequest == null || personIdsRequest.PersonalIdentificationNumbers == null || !personIdsRequest.PersonalIdentificationNumbers.Any())
        {
            throw new ArgumentException("Personal identification numbers cannot be null or empty.", nameof(personIdsRequest));
        }

        // Use the correct path relative to the base address
        var response = await _httpClient.PostAsJsonAsync("/api/v1/vehicles/personal/", personIdsRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error fetching car registrations: {response.ReasonPhrase}");
        }
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(data))
        {
            throw new InvalidOperationException("No car registration data found for the provided personal identification numbers.");
        }
        var cars = System.Text.Json.JsonSerializer.Deserialize<List<CarDto>>(data);
        if (cars == null)
        {
            throw new InvalidOperationException("Deserialized car object is null.");
        }
        return cars;
    }

}
