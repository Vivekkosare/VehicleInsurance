namespace InsuranceAPI.Features.Insurance.Entities;

public class InsuranceProduct: BaseEntity
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}
