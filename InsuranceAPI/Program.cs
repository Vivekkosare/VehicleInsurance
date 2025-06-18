using InsuranceAPI.Data;
using InsuranceAPI.Features.Insurance.Endpoints;
using InsuranceAPI.Features.Insurance.Repositories;
using InsuranceAPI.Features.Insurance.Services;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.ServiceRegistrations;
using VehicleInsurance.Shared.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using InsuranceAPI.Features.Insurance.Validators;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

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

// Register FluentValidation validators for InsuranceInput
builder.Services.AddValidatorsFromAssemblyContaining<InsuranceInputValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    // Use URL segment versioning (e.g., /api/v1/)
    options.ApiVersionReader = new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader();
});

var app = builder.Build();

// Apply migrations and seed data at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>();
    if (databaseCreator is RelationalDatabaseCreator)
    {
        dbContext.Database.Migrate();
    }
    // Seed data is handled by EF Core if configured with HasData
}

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
