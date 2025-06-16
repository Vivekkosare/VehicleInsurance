namespace InsuranceAPI.DTOs;

public record InsuranceInput(
    Guid InsuranceProductId,
    string PersonalIdentificationNumber,
    string InsuredItem,
    DateTime StartDate,
    DateTime EndDate
);
