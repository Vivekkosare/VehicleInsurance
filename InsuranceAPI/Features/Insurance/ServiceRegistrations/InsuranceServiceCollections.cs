using FluentValidation;
using FluentValidation.AspNetCore;
using InsuranceAPI.Features.Insurance.Pricing;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.Features.Insurance.Services;
using InsuranceAPI.Features.Insurance.Validators;

namespace InsuranceAPI.Features.Insurance.ServiceRegistrations;

public static class InsuranceServiceCollections
{
    public static IServiceCollection AddInsuranceServices(this IServiceCollection services)
    {

        //Register Pricing Services
        services.AddTransient<IPriceCalculator, DefaultPriceCalculator>();
        services.AddTransient<DefaultPriceCalculator>();
        services.AddTransient<DiscountedPriceCalculator>();
        services.AddTransient<IPriceCalculatorFactory, PriceCalculatorFactory>();

        // Register Insurance Services
        services.AddScoped<IInsuranceRepository, InsuranceRepository>();
        services.AddScoped<IInsuranceService, InsuranceService>();

        // Register FluentValidation validators for InsuranceInput
        services.AddValidatorsFromAssemblyContaining<InsuranceInputValidator>();
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
