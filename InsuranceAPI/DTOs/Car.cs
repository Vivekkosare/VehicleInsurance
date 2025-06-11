namespace InsuranceAPI.DTOs;

public record Car(string Name,
    string RegistrationNumber,
    string Make,
    string Model,
    int Year,
    string Color,
    DateTime RegistrationDate,
    string Owner,
    Guid OwnerId)
{
    public static implicit operator bool(Car v)
    {
        throw new NotImplementedException();
    }
}

