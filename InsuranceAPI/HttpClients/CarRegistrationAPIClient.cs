using System.Net.Http.Headers;
using InsuranceAPI.DTOs;

namespace InsuranceAPI.HttpClients;

public class CarRegistrationAPIClient
{
private readonly HttpClient _httpClient;

    public CarRegistrationAPIClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    public async Task<Car> GetCarRegistrationAsync(string personalIdentificationNumber)
    {
        if (string.IsNullOrEmpty(personalIdentificationNumber))
        {
            throw new ArgumentException("Personal identification number cannot be null or empty.", nameof(personalIdentificationNumber));
        }

        var response = await _httpClient.GetAsync($"api/CarRegistration/{personalIdentificationNumber}");
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
        var car = System.Text.Json.JsonSerializer.Deserialize<Car>(data);
        if (car == null)
        {
            throw new InvalidOperationException("Deserialized car object is null.");
        }
        return car;
    }
}
