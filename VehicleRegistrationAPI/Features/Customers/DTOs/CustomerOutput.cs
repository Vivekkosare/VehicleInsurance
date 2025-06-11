namespace VehicleRegistrationAPI.Features.Customers.DTOs;

public record CustomerOutput(Guid Id, string Name, string Email, string PhoneNumber);
