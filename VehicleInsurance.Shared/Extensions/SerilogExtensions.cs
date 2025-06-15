using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace VehicleInsurance.Shared.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder RegisterSerilog(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(configuration.GetValue<string>("SerilogPath") ??
                    throw new ArgumentNullException("SerilogPath", "Log file path cannot be null."), rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateLogger();

        builder.Host.UseSerilog(logger);

        return builder;
    }
}
