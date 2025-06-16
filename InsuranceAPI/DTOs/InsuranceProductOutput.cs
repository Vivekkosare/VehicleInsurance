namespace InsuranceAPI.DTOs;

public record InsuranceProductOutput(Guid InsuranceProductId,
    string Name,
    string Code,
    decimal Price);
