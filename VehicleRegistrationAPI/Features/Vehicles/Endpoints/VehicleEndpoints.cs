using VehicleRegistrationAPI.Features.Vehicles.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.Services;

namespace VehicleRegistrationAPI.Features.Vehicles.Endpoints;

public static class VehicleEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/vehicles");
        group.WithTags("Vehicles");

        /// <summary>
        /// Get a vehicle by its ID.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle.</param>
        /// <returns>A vehicle object if found, otherwise a 404 Not Found response.</returns>
        /// <response code="200">Returns the vehicle details.</response>
        /// <response code="404">If the vehicle with the specified ID does not exist.</response>
        group.MapGet("{vehicleId:guid}", async (Guid vehicleId, IVehicleService vehicleService) =>
        {
            var vehicle = await vehicleService.GetVehicleByIdAsync(vehicleId);
            return vehicle is not null ? Results.Ok(vehicle) : Results.NotFound();
        })
        .WithName("GetVehicleById")
        .Produces<VehicleOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Get a vehicle by its registration number.
        /// </summary>
        /// <param name="registrationNumber">The registration number of the vehicle.</param>
        /// <returns>A vehicle object if found, otherwise a 404 Not Found response.</returns>
        /// <response code="200">Returns the vehicle details.</response>
        /// <response code="404">If the vehicle with the specified registration number does not exist.</response>
        group.MapGet("registration/{registrationNumber}", async (string registrationNumber, IVehicleService vehicleService) =>
        {
            var vehicle = await vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
            return vehicle is not null ? Results.Ok(vehicle) : Results.NotFound();
        })
        .WithName("GetVehicleByRegistrationNumber")
        .Produces<VehicleOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Get all vehicles.
        /// </summary>
        /// <returns>A list of all vehicles.</returns>
        /// <response code="200">Returns a list of all vehicles.</response>
        /// <response code="404">If no vehicles are found.</response>
        group.MapGet("/", async (IVehicleService vehicleService) =>
        {
            var vehicles = await vehicleService.GetAllVehiclesAsync();
            return Results.Ok(vehicles);
        })
        .WithName("GetAllVehicles")
        .Produces<IEnumerable<VehicleOutput>>(StatusCodes.Status200OK);


        /// <summary>
        /// Add a new vehicle.
        /// </summary>
        /// <param name="vehicleInput">The vehicle data to add.</param>
        /// <returns>The created vehicle object.</returns>
        /// <response code="201">Returns the created vehicle object.</response>
        /// <response code="400">If the input data is invalid.</response>
        group.MapPost("/add", async (VehicleInput vehicleInput, IVehicleService vehicleService) =>
        {
            var createdVehicle = await vehicleService.AddVehicleAsync(vehicleInput);
            return Results.Created($"/vehicles/{createdVehicle.Id}", createdVehicle);
        })
        .WithName("AddVehicle")
        .Accepts<VehicleInput>("application/json")
        .Produces<VehicleOutput>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);


        /// <summary>
        /// Update an existing vehicle by ID.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to update.</param>
        /// <param name="vehicleInput">The updated vehicle data.</param>
        /// <returns>The updated vehicle object if successful, otherwise a 404 Not Found response.</returns>
        /// <response code="200">Returns the updated vehicle object.</response>
        /// <response code="404">If the vehicle with the specified ID does not exist.</response>
        group.MapPut("/{vehicleId:guid}", async (Guid vehicleId, VehicleInput vehicleInput, IVehicleService vehicleService) =>
        {
            var updatedVehicle = await vehicleService.UpdateVehicleAsync(vehicleId, vehicleInput);
            return updatedVehicle is not null ? Results.Ok(updatedVehicle) : Results.NotFound();
        })
        .WithName("UpdateVehicle")
        .Accepts<VehicleInput>("application/json")
        .Produces<VehicleOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Delete a vehicle by ID.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to delete.</param>
        /// <returns>No content if successful, otherwise a 404 Not Found response.</returns>
        /// <response code="204">If the vehicle was successfully deleted.</response>
        /// <response code="404">If the vehicle with the specified ID does not exist.</response>
        group.MapDelete("/{vehicleId:guid}", async (Guid vehicleId, IVehicleService vehicleService) =>
        {
            await vehicleService.DeleteVehicleAsync(vehicleId);
            return Results.NoContent();
        })
        .WithName("DeleteVehicle")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}