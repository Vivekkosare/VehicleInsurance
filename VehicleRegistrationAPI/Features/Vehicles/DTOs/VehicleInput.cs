namespace VehicleRegistrationAPI.Features.Vehicles.DTOs;

public record VehicleInput(
    string Name,
    string RegistrationNumber,
    string Make,
    string Model,
    int Year,
    string Color,
    DateTime RegistrationDate,
    Guid OwnerId
);

