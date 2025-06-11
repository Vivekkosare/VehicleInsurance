using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Services;

namespace InsuranceAPI.Features.Insurance.Extensions;

public static class InsuranceEndpoints
{
    public static void MapInsuranceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/insurances");

        /// <summary>
        /// Maps the insurance endpoints to the application.
        /// </summary>
        group.MapGet("/", async (IInsuranceService insuranceService) =>
        {
            var insurances = await insuranceService.GetAllInsurancesAsync();
            return Results.Ok(insurances);

        }).Accepts<Entities.Insurance>("application/json")
          .Produces<IEnumerable<Entities.Insurance>>(StatusCodes.Status200OK)
          .WithName("GetAllInsurances")
          .WithSummary("Retrieves all insurances");

        /// <summary>
        /// Maps the endpoint to retrieve an insurance by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the insurance.</param>
        /// <param name="insuranceService">The service to handle insurance operations.</param>
        /// <returns>An action result containing the insurance or a not found response.</returns>
        group.MapGet("/{id:guid}", async (Guid id, IInsuranceService insuranceService) =>
        {
            var insurance = await insuranceService.GetInsuranceByIdAsync(id);
            return insurance is not null ? Results.Ok(insurance) : Results.NotFound();

        }).Accepts<Entities.Insurance>("application/json")
          .Produces<Entities.Insurance>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound)
          .WithName("GetInsuranceById")
          .WithSummary("Retrieves an insurance by its ID");


        /// <summary>
        /// Maps the endpoint to add a new insurance.
        /// </summary>
        /// <param name="insurance">The insurance to be added.</param>
        /// <param name="insuranceService">The service to handle insurance operations.</param>
        /// <returns>An action result containing the created insurance or a bad request response.</returns>
        group.MapPost("/", async (Entities.Insurance insurance, IInsuranceService insuranceService) =>
        {
            var createdInsurance = await insuranceService.AddInsuranceAsync(insurance);
            return Results.Created($"/api/insurances/{createdInsurance.Id}", createdInsurance);

        }).Accepts<Entities.Insurance>("application/json")
          .Produces<Entities.Insurance>(StatusCodes.Status201Created)
          .WithName("AddInsurance")
          .WithSummary("Adds a new insurance");


        /// <summary>
        /// Maps the endpoint to retrieve insurances by personal identification number.
        /// </summary>
        /// <param name="personalIdentificationNumber">The personal identification number to filter insurances.</param>
        /// <param name="insuranceService">The service to handle insurance operations.</param>
        /// <returns>An action result containing a list of insurances or a not found response.</returns>
        group.MapGet("/pin/{personalIdentificationNumber}", async (string personalIdentificationNumber, IInsuranceService insuranceService) =>
        {
            var insurances = await insuranceService.GetInsurancesByPersonalIdentificationNumberAsync(personalIdentificationNumber);
            return Results.Ok(insurances);

        }).Accepts<string>("application/json")
          .Produces<IEnumerable<InsuranceResponse>>(StatusCodes.Status200OK)
          .WithName("GetInsurancesByPersonalIdentificationNumber")
          .WithSummary("Retrieves insurances by personal identification number");
    }
}
