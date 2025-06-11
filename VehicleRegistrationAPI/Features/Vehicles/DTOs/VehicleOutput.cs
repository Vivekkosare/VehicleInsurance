using VehicleRegistrationAPI.Features.Customers.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.DTOs;

public record VehicleOutput(
    Guid Id,
    string Name,
    string RegistrationNumber,
    string Make,
    string Model,
    int Year,
    string Color,
    DateTime RegistrationDate,
    CustomerOutput Owner);
