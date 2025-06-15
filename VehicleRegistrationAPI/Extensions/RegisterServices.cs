using VehicleRegistrationAPI.Features.Customers.Repositories;
using VehicleRegistrationAPI.Features.Customers.Services;
using VehicleRegistrationAPI.Features.Vehicles.Repositories;
using VehicleRegistrationAPI.Features.Vehicles.Services;

namespace VehicleRegistrationAPI.Extensions;

public static class RegisterServices
{
    public static IServiceCollection AddVehicleRegistrationServices(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleService, VehicleService>();

        return services;
    }
}
