using Polly;
using Polly.Extensions.Http;

namespace InsuranceAPI.ServiceRegistrations;

public static class HttpClientExtensions
{
    public static IServiceCollection AddVehicleRegistrationAPIHttpClient(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddHttpClient<HttpClients.ICarRegistrationAPIClient, HttpClients.CarRegistrationAPIClient>()
                .ConfigureHttpClient(client =>
                {
                    var baseUrl = configuration.GetValue<string>("VehicleRegistrationAPIUrl")
                        ?? configuration["VehicleRegistrationApi:BaseUrl"]
                        ?? throw new InvalidOperationException("VehicleRegistrationAPIUrl or VehicleRegistrationApi:BaseUrl is not configured.");
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        sleepDurationProvider: retryAttempt => retryAttempt switch
                        {
                            1 => TimeSpan.FromMilliseconds(500),
                            2 => TimeSpan.FromMilliseconds(1000),
                            3 => TimeSpan.FromMilliseconds(2000),
                            _ => TimeSpan.FromMilliseconds(2000)
                        }
                    ))
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: 2,
                        durationOfBreak: TimeSpan.FromSeconds(30)
                    ));
        return services;
    }
}
