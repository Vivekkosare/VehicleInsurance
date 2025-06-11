namespace InsuranceAPI.DTOs;

public record InsuranceResponse(string PersonalIdentificationNumber,
    InsuranceProductResponse InsuranceProduct,    
    DateTime StartDate,
    DateTime EndDate,
    Car? Car);

