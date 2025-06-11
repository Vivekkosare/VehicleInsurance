namespace InsuranceAPI.Features.Insurance.Entities;

public class Insurance: BaseEntity
{
    public Guid Id { get; set; }
    public Guid InsuranceProductId { get; set; }
    public InsuranceProduct InsuranceProduct { get; set; } // Navigation property
    public string PersonalIdentificationNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
