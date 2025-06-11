namespace VehicleRegistrationAPI.Entities;

public class Customer : BaseEntity
{
    public Customer()
    {
        Vehicles = new HashSet<Vehicle>();
    }

    public string Name { get; set; }
    public string PersonalIdentificationNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; }
}
