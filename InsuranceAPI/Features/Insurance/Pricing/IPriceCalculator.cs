using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Features.Insurance.Pricing
{
    public interface IPriceCalculator
    {
        decimal CalculatePrice(InsuranceProduct insuranceProduct);
    }
}