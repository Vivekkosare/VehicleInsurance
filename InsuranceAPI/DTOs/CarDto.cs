using System.Text.Json.Serialization;

namespace InsuranceAPI.DTOs;

public record CarDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("registrationNumber")] string RegistrationNumber,
    [property: JsonPropertyName("make")] string Make,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("registrationDate")] DateTime RegistrationDate,
    [property: JsonPropertyName("owner")] OwnerDto Owner);
