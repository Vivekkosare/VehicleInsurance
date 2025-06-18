using FluentValidation;
using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
namespace InsuranceAPI.Features.FeatureManagement.Endpoints
{
    public static class FeatureManagementEndpoints
    {
        public static IEndpointRouteBuilder MapFeatureManagementEndpoints(this IEndpointRouteBuilder builder)
        {
            // Use versioned route group
            var group = builder.MapGroup("/api/v{version:apiVersion}/featuretoggles")
                .WithTags("Feature Management");

            /// <summary>
            /// Get all feature toggles.
            /// </summary>
            /// returns A list of feature toggles.
            /// <response code="200">Returns a list of feature toggles.</response>
            /// <response code="500">If an error occurs while retrieving the feature toggles.</response>
            group.MapGet("/", async ([FromServices]IFeatureManagementService service) =>
            {
                var result = await service.GetAllFeatureTogglesAsync();
                return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error);
            })
            .WithName("GetAllFeatureToggles")
            .Produces<IEnumerable<FeatureToggleOutput>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


            /// <summary>
            /// Add a new feature toggle.
            /// </summary>  
            /// <param name="featureToggleInput">The feature toggle input data.</param>
            /// <returns>The created feature toggle.</returns>
            /// <response code="201">Returns the created feature toggle.</response>
            /// <response code="400">If the input data is invalid.</response>
            /// <response code="500">If an error occurs while adding the feature toggle.</response>
            group.MapPost("/", async ([FromBody] FeatureToggleInput featureToggleInput,
            [FromServices] IFeatureManagementService service,
            [FromServices] IValidator<FeatureToggleInput> validator) =>
            {
                var validationResult = await validator.ValidateAsync(featureToggleInput);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await service.AddFeatureToggleAsync(featureToggleInput);
                return result.IsSuccess ? Results.CreatedAtRoute("GetFeatureToggleById", new { id = result.Value.Id }, result.Value) :
                    Results.Problem(result.Error);
            })
            .WithName("AddFeatureToggle")
            .Produces<FeatureToggleOutput>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


            /// <summary>
            /// Get a feature toggle by its ID.
            /// </summary>
            /// <param name="id">The unique identifier of the feature toggle.</param>
            /// <returns>The feature toggle if found, otherwise a 404 Not Found response.</returns>
            /// <response code="200">Returns the feature toggle.</response>
            /// <response code="404">If the feature toggle with the specified ID is not found.</response>
            /// <response code="500">If an error occurs while retrieving the feature toggle.</response>
            group.MapGet("/{id:guid}", async (Guid id, [FromServices] IFeatureManagementService service) =>
            {
                if (id == Guid.Empty)
                {
                    return Results.BadRequest("Invalid feature toggle ID.");
                }
                var result = await service.GetFeatureToggleByIdAsync(id);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error);
            })
            .WithName("GetFeatureToggleById")
            .Produces<FeatureToggleOutput>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


            /// <summary>
            /// Delete a feature toggle by its ID.
            /// </summary>
            /// <param name="id">The unique identifier of the feature toggle.</param>
            /// <returns>No content if the deletion is successful, otherwise a 404 Not Found or 500 Internal Server Error response.</returns>
            /// <response code="204">If the feature toggle is successfully deleted.</response>
            /// <response code="404">If the feature toggle with the specified ID is not found.</response>
            /// <response code="500">If an error occurs while deleting the feature toggle.</response>
            group.MapDelete("/{id:guid}", async (Guid id, [FromServices] IFeatureManagementService service) =>
            {
                var result = await service.DeleteFeatureToggleAsync(id);
                return result.IsSuccess ? Results.NoContent() : Results.Problem(result.Error);
            })
            .WithName("DeleteFeatureToggle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


            /// <summary>
            /// Update a feature toggle by its ID.
            /// </summary>
            /// <param name="id">The unique identifier of the feature toggle.</param>
            /// <param name="featureToggleInput">The feature toggle input data.</param>
            /// <returns>The updated feature toggle if successful, otherwise a 400 Bad Request, 404 Not Found, or 500 Internal Server Error response.</returns>
            /// <response code="200">Returns the updated feature toggle.</response>
            /// <response code="400">If the input data is invalid.</response>
            /// <response code="404">If the feature toggle with the specified ID is not found.</response>
            /// <response code="500">If an error occurs while updating the feature toggle.</response>
            group.MapPut("/{id:guid}", async (Guid id, [FromBody] FeatureToggleInput featureToggleInput,
            [FromServices] IFeatureManagementService service,
            [FromServices] IValidator<FeatureToggleInput> validator) =>
            {
                var validationResult = await validator.ValidateAsync(featureToggleInput);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await service.PatchFeatureToggleAsync(id, featureToggleInput);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error);
            })
            .WithName("UpdateFeatureToggle")
            .Produces<FeatureToggleOutput>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            return group;
        }
    }
}
