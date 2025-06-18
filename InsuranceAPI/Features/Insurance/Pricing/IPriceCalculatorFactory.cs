namespace InsuranceAPI.Features.Insurance.Pricing;

public interface IPriceCalculatorFactory
{
    public IPriceCalculator GetPriceCalculator(bool applyDiscount);
}
