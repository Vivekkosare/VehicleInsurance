namespace InsuranceAPI.Features.Insurance.DTOs;

public record InsuranceInput(
    Guid InsuranceProductId,
    string PersonalIdentificationNumber,
    string InsuredItemIdentity,
    DateTime StartDate,
    DateTime EndDate
);
