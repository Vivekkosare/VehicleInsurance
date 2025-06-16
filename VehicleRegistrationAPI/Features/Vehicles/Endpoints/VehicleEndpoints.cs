using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Shared;
using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.Services;
using FluentValidation;

namespace VehicleRegistrationAPI.Features.Vehicles.Endpoints;

public static class VehicleEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/vehicles");
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
            return vehicle.IsSuccess ? Results.Ok(vehicle.Value) : Results.Problem(vehicle.Error);
        })
        .WithName("GetVehicleById")
        .Produces<VehicleOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);


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
            return vehicle.IsSuccess ? Results.Ok(vehicle.Value) : Results.Problem(vehicle.Error);
        })
        .WithName("GetVehicleByRegistrationNumber")
        .Produces<VehicleOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);


        /// <summary>
        /// Get all vehicles.
        /// </summary>
        /// <returns>A list of all vehicles.</returns>
        /// <response code="200">Returns a list of all vehicles.</response>
        /// <response code="404">If no vehicles are found.</response>
        group.MapGet("/", async (IVehicleService vehicleService) =>
        {
            var vehicles = await vehicleService.GetAllVehiclesAsync();
            return vehicles.IsSuccess ? Results.Ok(vehicles.Value) : Results.Problem(vehicles.Error);
        })
        .WithName("GetAllVehicles")
        .Produces<IEnumerable<VehicleOutput>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);



        /// <summary>
        /// Add a new vehicle.
        /// </summary>
        /// <param name="vehicleInput">The vehicle data to add.</param>
        /// <returns>The created vehicle object.</returns>
        /// <response code="201">Returns the created vehicle object.</response>
        /// <response code="400">If the input data is invalid.</response>
        group.MapPost("/add", async (VehicleInput vehicleInput, IVehicleService vehicleService, IValidator<VehicleInput> validator) =>
        {
            var validationResult = await validator.ValidateAsync(vehicleInput);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            var createdVehicle = await vehicleService.AddVehicleAsync(vehicleInput);
            return createdVehicle.IsSuccess ? Results.Created($"/vehicles/{createdVehicle.Value.Id}", createdVehicle.Value)
                        : Results.Problem(createdVehicle.Error);
        })
        .WithName("AddVehicle")
        .Accepts<VehicleInput>("application/json")
        .Produces<VehicleOutput>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status500InternalServerError);


        /// <summary>
        /// Update an existing vehicle by ID.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to update.</param>
        /// <param name="vehicleInput">The updated vehicle data.</param>
        /// <returns>The updated vehicle object if successful, otherwise a 404 Not Found response.</returns>
        /// <response code="200">Returns the updated vehicle object.</response>
        /// <response code="404">If the vehicle with the specified ID does not exist.</response>
        group.MapPut("/{vehicleId:guid}", async (Guid vehicleId, VehicleInput vehicleInput, IVehicleService vehicleService, IValidator<VehicleInput> validator) =>
        {
            var validationResult = await validator.ValidateAsync(vehicleInput);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            var updatedVehicle = await vehicleService.UpdateVehicleAsync(vehicleId, vehicleInput);
            return updatedVehicle.IsSuccess ? Results.Ok(updatedVehicle.Value) : Results.Problem(updatedVehicle.Error);
        })
        .WithName("UpdateVehicle")
        .Accepts<VehicleInput>("application/json")
        .Produces<VehicleOutput>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status500InternalServerError);


        /// <summary>
        /// Delete a vehicle by ID.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to delete.</param>
        /// <returns>No content if successful, otherwise a 404 Not Found response.</returns>
        /// <response code="204">If the vehicle was successfully deleted.</response>
        /// <response code="404">If the vehicle with the specified ID does not exist.</response>
        group.MapDelete("/{vehicleId:guid}", async (Guid vehicleId, IVehicleService vehicleService) =>
        {
            var vehicleDeleted = await vehicleService.DeleteVehicleAsync(vehicleId);
            return vehicleDeleted.IsSuccess ? Results.NoContent() : Results.Problem(vehicleDeleted.Error);
        })
        .WithName("DeleteVehicle")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Get all vehicles by Personal Identification Number for a customer.
        /// </summary>
        /// <param name="personalIdentificationNumber">The personal identification number of the customer.</param>
        /// <returns>A list of vehicles associated with the provided personal identification number.</returns>
        /// <response code="200">Returns a list of vehicles.</response>
        /// <response code="404">If no vehicles are found for the provided personal identification number.</response>
        group.MapGet("/personal/{personalIdentificationNumber}", async (string personalIdentificationNumber, IVehicleService vehicleService) =>
        {
            var vehicles = await vehicleService.GetVehiclesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
            return vehicles.IsSuccess ? Results.Ok(vehicles.Value) : Results.Problem(vehicles.Error);
        })
        .WithName("GetVehiclesByPersonalIdentificationNumber")
        .Produces<IEnumerable<VehicleOutput>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Get all vehicles by multiple personal identifiers.
        /// </summary>
        /// <param name="personIds">The request containing multiple personal identifiers.</param>
        /// <returns>A list of vehicles associated with the provided personal identifiers.</returns>
        /// <response code="200">Returns a list of vehicles.</response>
        /// <response code="404">If no vehicles are found for the provided personal identifiers.</response>
        group.MapPost("/personal/", async ([FromBody] PersonIdentifiersRequest personIds, IVehicleService vehicleService) =>
        {
            var vehiclesByOwners = await vehicleService.GetVehiclesByPersonalIdsAsync(personIds);
            return vehiclesByOwners.IsSuccess ? Results.Ok(vehiclesByOwners.Value) : Results.Problem(vehiclesByOwners.Error);
        })
        .WithName("GetVehiclesByPersonalIdentifiers")
        .Produces<IEnumerable<VehicleOutput>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}