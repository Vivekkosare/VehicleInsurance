namespace InsuranceAPI.Features.Insurance.DTOs;

public record InsuranceProductOutput(Guid InsuranceProductId,
    string Name,
    string Code,
    decimal Price,
    decimal DiscountPercentage);
