using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Features.Insurance.Pricing
{
    public class DiscountedPriceCalculator : IPriceCalculator
    {
        public decimal CalculatePrice(InsuranceProduct insuranceProduct)
        {
            return insuranceProduct.Price * (1 - insuranceProduct.DiscountPercentage / 100);
        }
    }
}