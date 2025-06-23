using FluentValidation;
using FluentValidation.AspNetCore;
using InsuranceAPI.Features.FeatureManagement.Repositories;
using InsuranceAPI.Features.FeatureManagement.Services;
using InsuranceAPI.Features.FeatureManagement.Validators;

namespace InsuranceAPI.Features.FeatureManagement.ServiceRegistrations;

public static class FeatureManagementServiceCollections
{
    public static IServiceCollection AddFeatureManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IFeatureManagementRepository, FeatureManagementRepository>();
        services.AddScoped<IFeatureManagementService, FeatureManagementService>();

        services.AddValidatorsFromAssemblyContaining<FeatureToggleNameInputValidator>();
        services.AddValidatorsFromAssemblyContaining<FeatureToggleInputValidator>();
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
