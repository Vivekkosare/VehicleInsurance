using System.Text.Json.Serialization;

namespace VehicleInsurance.Shared.DTOs;

public record CustomerOutput(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("personalIdentificationNumber")] string PersonalIdentificationNumber,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber
);

