namespace InsuranceAPI.ServiceRegistrations;

public static class HttpClientExtensions
{
    public static IServiceCollection AddVehicleRegistrationAPIHttpClient(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddHttpClient<HttpClients.ICarRegistrationAPIClient, HttpClients.CarRegistrationAPIClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("VehicleRegistrationAPIUrl") ??
                            throw new InvalidOperationException("VehicleRegistrationAPIUrl is not configured."));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                });
        return services;
    }
}
