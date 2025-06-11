namespace VehicleRegistrationAPI.Entities;

public class Vehicle : BaseEntity
{
    public string Name { get; set; }
    public string RegistrationNumber { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public DateTime RegistrationDate { get; set; }
    public Guid OwnerId { get; set; }
    public Customer Owner { get; set; }
}   