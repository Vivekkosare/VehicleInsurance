using System.Text.Json.Serialization;

namespace InsuranceAPI.Features.Insurance.DTOs;

public record OwnerDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("personalIdentificationNumber")] string PersonalIdentificationNumber,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber);
