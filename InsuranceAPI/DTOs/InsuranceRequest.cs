namespace InsuranceAPI.DTOs;

public record InsuranceRequest(Guid InsuranceProductId,
    string PersonalIdentificationNumber,
    DateTime StartDate,
    DateTime EndDate);
    