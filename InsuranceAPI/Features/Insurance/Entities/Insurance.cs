using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Features.Insurance.Entities;

public class Insurance: BaseEntity
{
    public Guid InsuranceProductId { get; set; }
    public InsuranceProduct InsuranceProduct { get; set; } // Navigation property
    public string PersonalIdentificationNumber { get; set; }
    public string InsuredItem { get; set; }
    public decimal Price { get; set; }
    public bool DiscountApplied { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
