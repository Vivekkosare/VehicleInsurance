using System.Text.Json.Serialization;

namespace InsuranceAPI.Features.Insurance.DTOs;

public record InsuranceOutput(Guid Id,
    string PersonalIdentificationNumber,
    InsuranceProductOutput InsuranceProduct,    
    DateTime StartDate,
    DateTime EndDate,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] CarDto? Car);
