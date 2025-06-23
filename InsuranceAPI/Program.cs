using InsuranceAPI.Data;
using InsuranceAPI.Features.FeatureManagement.ServiceRegistrations;
using InsuranceAPI.Features.FeatureManagement.Endpoints;
using InsuranceAPI.Features.Insurance.Endpoints;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.Features.Insurance.Services;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.ServiceRegistrations;
using VehicleInsurance.Shared.Extensions;
using InsuranceAPI.Features.Insurance.ServiceRegistrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

//Register Serilog for logging
builder.RegisterSerilog(builder.Configuration);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Register the DbContext with SQL Server or InMemory for tests
if (Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true")
{
    builder.Services.AddDbContext<InsuranceDbContext>(options =>
    {
        options.UseInMemoryDatabase("TestInsuranceDb");
    });
}
else
{
    builder.Services.AddDbContext<InsuranceDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
}

//Add and Register the Redis cache service
builder.Services.AddRedisCache(builder.Configuration);

// Register the repositories and services
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();

//Register the http client for the Vehicle Registration API
builder.Services.AddVehicleRegistrationAPIHttpClient(builder.Configuration);

// Register feature management services
builder.Services.AddFeatureManagementServices();
builder.Services.AddInsuranceServices();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Map the insurance endpoints
app.MapInsuranceEndpoints();
// Map the feature management endpoints
app.MapFeatureManagementEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler(error =>
    {
        // Configure the exception handler
        error.ConfigureExceptionHandler();
    });
}

// Configure the HTTP request pipeline.
app.Run();

namespace InsuranceAPI
{
    public partial class Program { }
}
