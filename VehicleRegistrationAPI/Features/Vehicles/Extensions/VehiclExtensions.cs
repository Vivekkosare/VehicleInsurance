using VehicleRegistrationAPI.Entities;
using VehicleRegistrationAPI.Features.Customers.Extensions;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Extensions;

public static class VehiclExtensions
{
    public static VehicleOutput ToVehicleOutput(this Vehicle vehicle)
    {
        if (vehicle == null) return null;

        return new VehicleOutput(
            vehicle.Id,
            vehicle.Name,
            vehicle.RegistrationNumber,
            vehicle.Make,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.RegistrationDate,
            vehicle.Owner?.ToCustomerOutput());
    }

    public static Vehicle ToVehicleEntity(this VehicleInput vehicleInput)
    {
        if (vehicleInput == null) return null;

        return new Vehicle
        {
            Name = vehicleInput.Name,
            RegistrationNumber = vehicleInput.RegistrationNumber,
            Make = vehicleInput.Make,
            Model = vehicleInput.Model,
            Year = vehicleInput.Year,
            Color = vehicleInput.Color,
            RegistrationDate = vehicleInput.RegistrationDate,
            OwnerId = vehicleInput.OwnerId
        };
    }
}
