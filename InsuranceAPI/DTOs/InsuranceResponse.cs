using System.Text.Json.Serialization;

namespace InsuranceAPI.DTOs;

public record InsuranceResponse(string PersonalIdentificationNumber,
    InsuranceProductResponse InsuranceProduct,    
    DateTime StartDate,
    DateTime EndDate,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] CarDto? Car);

