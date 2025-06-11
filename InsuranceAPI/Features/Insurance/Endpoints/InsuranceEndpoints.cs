using InsuranceAPI.Features.Insurance.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAPI.Features.Insurance.Endpoints;

public static class InsuranceEndpoints
{
    public static void MapInsuranceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/insurances");

        /// <summary>
        /// Maps the insurance endpoints for managing insurance records.
        /// </summary>
        /// <param name="app">The WebApplication instance to map the endpoints to.</param>
        /// <returns>A group of endpoints for insurance operations.</returns>
        group.MapGet("/", async ([FromServices]IInsuranceService insuranceService) =>
        {
            var insurances = await insuranceService.GetAllInsurancesAsync();
            return Results.Ok(insurances);
        })
        .WithName("GetAllInsurances")
        .Produces<IEnumerable<Entities.Insurance>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Gets an insurance record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the insurance record.</param>
        /// <returns>An insurance object if found, otherwise a 404 Not Found response.</returns>
        group.MapGet("/{id:guid}", async (Guid id, [FromServices]IInsuranceService insuranceService) =>
        {
            var insurance = await insuranceService.GetInsuranceByIdAsync(id);
            return insurance is not null ? Results.Ok(insurance) : Results.NotFound();
        })
        .WithName("GetInsuranceById")
        .Produces<Entities.Insurance>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Gets an insurance record by its policy number.
        /// </summary>
        /// <param name="policyNumber">The policy number of the insurance record.</param>
        /// <returns>An insurance object if found, otherwise a 404 Not Found response.</returns>
        group.MapPost("/", async ([FromBody]Entities.Insurance insurance, [FromServices]IInsuranceService insuranceService) =>
        {
            if (insurance is null)
            {
                return Results.BadRequest("Insurance cannot be null.");
            }

            var newInsurance = await insuranceService.AddInsuranceAsync(insurance);
            return Results.Created($"/api/insurances/{newInsurance.Id}", newInsurance);
        })
        .WithName("AddInsurance")
        .Produces<Entities.Insurance>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);


        /// <summary>
        /// Gets all insurance records associated with a specific personal identification number.
        /// </summary>
        /// <param name="personalIdentificationNumber">The personal identification number to filter insurances by.</param>
        /// <returns>A list of insurance records associated with the specified personal identification number.</returns>
        group.MapGet("/pin/{personalIdentificationNumber}",
            async (string personalIdentificationNumber, [FromServices] IInsuranceService insuranceService) =>
        {
            var insurances = await insuranceService.GetInsurancesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
            return Results.Ok(insurances);
        })
        .WithName("GetInsurancesByPersonalIdentificationNumber")
        .Produces<IEnumerable<Entities.Insurance>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
