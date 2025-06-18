namespace InsuranceAPI.Features.Insurance.Pricing
{
    public class PriceCalculatorFactory(DefaultPriceCalculator _defaultPriceCalculator,
        DiscountedPriceCalculator _discountedPriceCalculator) : IPriceCalculatorFactory
    {
        public IPriceCalculator GetPriceCalculator(bool applyDiscount)
        {
            return applyDiscount ? _discountedPriceCalculator :
                _defaultPriceCalculator;
        }
    }
}