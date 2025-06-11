namespace InsuranceAPI.DTOs;

public record InsuranceProductResponse(Guid InsuranceProductId,
    string Name,
    string Code,
    decimal Price);
