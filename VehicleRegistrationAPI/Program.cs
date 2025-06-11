using Microsoft.EntityFrameworkCore;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Features.Customers.Endpoints;
using VehicleRegistrationAPI.Features.Customers.Repositories;
using VehicleRegistrationAPI.Features.Customers.Services;
using VehicleRegistrationAPI.Features.Vehicles.Endpoints;
using VehicleRegistrationAPI.Features.Vehicles.Repositories;
using VehicleRegistrationAPI.Features.Vehicles.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Register the DbContext with SQL Server
builder.Services.AddDbContext<VehicleRegistrationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Register repositories and services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();


var app = builder.Build();

//Register the endpoints
app.MapCustomerEndpoints();
app.MapVehicleEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
