using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VehicleInsurance.Shared.Extensions;

public static class ExceptionHandlerExtensions
{
     public static IApplicationBuilder ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            // Log the exception if available
            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                Serilog.Log.Error(exceptionHandlerPathFeature.Error, "Unhandled exception occurred.");
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Please try again later or contact support."
            };
            await context.Response.WriteAsJsonAsync(problem);
        });
        return app;
    }
}
