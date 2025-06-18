using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Features.Customers.Endpoints;
using VehicleRegistrationAPI.Features.Vehicles.Endpoints;
using VehicleInsurance.Shared.Extensions;
using VehicleRegistrationAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Register Serilog for logging
builder.RegisterSerilog(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Conditionally register the DbContext with PostgreSQL or InMemory for tests
var useInMemory = Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true";
if (useInMemory)
{
    builder.Services.AddDbContext<VehicleRegistrationDbContext>(options =>
    {
        options.UseInMemoryDatabase("TestDb");
    }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
}
else
{
    //Register the DbContext with PostgreSQL
    builder.Services.AddDbContext<VehicleRegistrationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
}

//Add and Register the Redis cache service
builder.Services.AddRedisCache(builder.Configuration);

//Register repositories and services
builder.Services.AddVehicleRegistrationServices();

// Register FluentValidation validators for input DTOs
builder.Services.AddValidatorsFromAssemblyContaining<VehicleRegistrationAPI.Features.Vehicles.Validators.VehicleInputValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<VehicleRegistrationAPI.Features.Customers.Validators.CustomerInputValidator>();

// Optionally enable automatic validation for minimal APIs
builder.Services.AddFluentValidationAutoValidation();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader();
});

var app = builder.Build();

// Apply migrations and seed data at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VehicleRegistrationDbContext>();
    var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>();
    if (databaseCreator is RelationalDatabaseCreator)
    {
        dbContext.Database.Migrate();
    }
    // Seed data is handled by EF Core if configured with HasData
}

//Register the endpoints
app.MapCustomerEndpoints();
app.MapVehicleEndpoints();

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

namespace VehicleRegistrationAPI
{
    public partial class Program { }
}
