using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

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
        group.MapGet("/", async ([FromServices] IInsuranceService insuranceService) =>
        {
            var insurances = await insuranceService.GetAllInsurancesAsync();
            return insurances.IsSuccess
                ? Results.Ok(insurances.Value)
                : Results.Problem(
                    title: "Failed to retrieve insurances",
                    detail: insurances.Error ?? "An error occurred while retrieving insurances.",
                    statusCode: StatusCodes.Status404NotFound
                );
        })
        .WithName("GetAllInsurances")
        .Produces<IEnumerable<InsuranceOutput>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Gets an insurance record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the insurance record.</param>
        /// <returns>An insurance object if found, otherwise a 404 Not Found response.</returns>
        group.MapGet("/{id:guid}", async (Guid id, [FromServices] IInsuranceService insuranceService) =>
        {
            var insurance = await insuranceService.GetInsuranceByIdAsync(id);
            return insurance.IsSuccess ? Results.Ok(insurance.Value) : Results.Problem(
                    title: "Failed to retrieve insurances",
                    detail: insurance.Error ?? "An error occurred while retrieving insurance.",
                    statusCode: StatusCodes.Status404NotFound
                );
        })
        .WithName("GetInsuranceById")
        .Produces<InsuranceOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Gets an insurance record by its policy number.
        /// </summary>
        /// <param name="policyNumber">The policy number of the insurance record.</param>
        /// <returns>An insurance object if found, otherwise a 404 Not Found response.</returns>
        group.MapPost("/", async ([FromBody] InsuranceInput insuranceInput, [FromServices] IInsuranceService insuranceService, [FromServices] IValidator<InsuranceInput> validator) =>
        {
            var validationResult = await validator.ValidateAsync(insuranceInput);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            var newInsurance = await insuranceService.AddInsuranceAsync(insuranceInput);
            if (newInsurance.IsSuccess && newInsurance.Value != null)
            {
                return Results.Created($"/api/insurances/{newInsurance.Value.Id}", newInsurance.Value);
            }
            return Results.Problem(
                title: "Failed to add insurance",
                detail: newInsurance.Error ?? "An error occurred while adding insurance.",
                statusCode: StatusCodes.Status400BadRequest
            );
        })
        .WithName("AddInsurance")
        .Accepts<InsuranceInput>("application/json")
        .Produces<InsuranceOutput>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
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
            return insurances != null && insurances.Value != null && insurances.Value.Any()
                ? Results.Ok(insurances.Value)
                : Results.Problem(
                    title: "No insurances found",
                    detail: $"No insurance records found for personal identification number: {personalIdentificationNumber}.",
                    statusCode: StatusCodes.Status404NotFound
                );
        })
        .WithName("GetInsurancesByPersonalIdentificationNumber")
        .Produces<IEnumerable<InsuranceOutput>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Updates an existing insurance record.
        /// </summary>
        /// <param name="id">The unique identifier of the insurance record to update.</param>
        /// <param name="insuranceInput">The updated insurance data.</param>
        /// <returns>A 200 OK response if the update is successful, otherwise an error response.</returns>
        group.MapPut("/{id:guid}", async (Guid id, [FromBody] InsuranceInput insuranceInput, [FromServices] IInsuranceService insuranceService, [FromServices] IValidator<InsuranceInput> validator) =>
        {
            var validationResult = await validator.ValidateAsync(insuranceInput);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            // TODO: Implement update logic using insuranceService
            return Results.Ok(); // Placeholder
        })
        .WithName("UpdateInsurance")
        .Accepts<InsuranceInput>("application/json")
        .Produces<InsuranceOutput>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status400BadRequest);
    }
}
