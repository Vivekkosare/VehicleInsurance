using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Features.Insurance.Pricing
{
    public class DefaultPriceCalculator : IPriceCalculator
    {
        public decimal CalculatePrice(InsuranceProduct insuranceProduct)
        {
            return insuranceProduct.Price;
        }
    }
}