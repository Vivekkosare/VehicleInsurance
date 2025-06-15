using InsuranceAPI.Data;
using InsuranceAPI.Features.Insurance.Endpoints;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.Features.Insurance.Services;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.ServiceRegistrations;
using VehicleInsurance.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Register Serilog for logging
builder.RegisterSerilog(builder.Configuration);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Register the DbContext with SQL Server
builder.Services.AddDbContext<InsuranceDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Add and Register the Redis cache service
builder.Services.AddRedisCache(builder.Configuration);

// Register the repositories and services
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();


//Register the http client for the Vehicle Registration API
builder.Services.AddVehicleRegistrationAPIHttpClient(builder.Configuration);


var app = builder.Build();

// Map the insurance endpoints
app.MapInsuranceEndpoints();

app.UseExceptionHandler(error =>
{
    // Configure the exception handler
    error.ConfigureExceptionHandler();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection();
}


app.Run();
