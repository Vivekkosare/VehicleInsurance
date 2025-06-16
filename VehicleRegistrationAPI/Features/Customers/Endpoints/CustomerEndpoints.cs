using FluentValidation;
using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Features.Customers.DTOs;
using VehicleRegistrationAPI.Features.Customers.Services;

namespace VehicleRegistrationAPI.Features.Customers.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        // Define the base route for customer endpoints
        var group = app.MapGroup("/api/v1/customers").WithTags("Customers");

        /// <summary>
        /// Retrieves a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to retrieve.</param>
        /// <returns>The customer data if found, otherwise a 404 Not Found response.</returns>
        /// <response code="200">Returns the customer data.</response>
        /// <response code="404">If the customer with the specified ID does not exist.</response>
        group.MapGet("/{id:guid}", async (Guid id, ICustomerService customerService) =>
        {
            var customer = await customerService.GetCustomerByIdAsync(id);
            return customer.IsSuccess ? Results.Ok(customer.Value) : Results.Problem(customer.Error);
        })
        .WithName("GetCustomerById")
        .Produces<CustomerOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);


        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <returns>A list of all customers.</returns>
        /// <response code="200">Returns a list of all customers.</response>
        group.MapGet("/", async (ICustomerService customerService) =>
        {
            var customers = await customerService.GetAllCustomersAsync();
            return customers.IsSuccess ? Results.Ok(customers.Value) : Results.Problem(customers.Error);
        })
        .WithName("GetAllCustomers")
        .Produces<IEnumerable<CustomerOutput>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError); ;


        /// <summary>
        /// Adds a new customer.
        /// </summary>
        /// <param name="customerInput">The customer data to add.</param>
        /// <returns>The created customer data.</returns>
        /// <response code="201">Returns the created customer data.</response>
        /// <response code="400">If the input data is invalid.</response>
        group.MapPost("/add", async (CustomerInput customerInput, ICustomerService customerService, IValidator<CustomerInput> validator) =>
        {
            var validationResult = await validator.ValidateAsync(customerInput);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            var newCustomer = await customerService.AddCustomerAsync(customerInput);
            return newCustomer.IsSuccess ? Results.Created($"/{newCustomer.Value.Id}", newCustomer.Value) :
                Results.Problem(newCustomer.Error);
        })
        .WithName("AddCustomer")
        .Accepts<CustomerInput>("application/json")
        .Produces<CustomerOutput>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status500InternalServerError); ;

        /// <summary>
        /// Updates an existing customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to update.</param>
        /// <param name="customerInput">The updated customer data.</param>
        /// <returns>The updated customer data.</returns>
        /// <response code="200">Returns the updated customer data.</response>
        /// <response code="404">If the customer with the specified ID does not exist.</response>
        group.MapPut("/{id:guid}", async (Guid id, CustomerInput customerInput, ICustomerService customerService, IValidator<CustomerInput> validator) =>
        {
            var validationResult = await validator.ValidateAsync(customerInput);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            var updatedCustomer = await customerService.UpdateCustomerAsync(id, customerInput);
            return updatedCustomer.IsSuccess ? Results.Ok(updatedCustomer.Value) : Results.Problem(updatedCustomer.Error);
        })
        .WithName("UpdateCustomer")
        .Accepts<CustomerInput>("application/json")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status500InternalServerError); ;

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to delete.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the customer was successfully deleted.</response>
        /// <response code="404">If the customer with the specified ID does not exist.</response>
        group.MapDelete("/{id:guid}", async (Guid id, ICustomerService customerService) =>
        {
            var customerDeleted = await customerService.DeleteCustomerAsync(id);
            return customerDeleted.IsSuccess ? Results.NoContent() : Results.Problem(customerDeleted.Error);
        })
        .WithName("DeleteCustomer")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status500InternalServerError);


        /// <summary>
        /// Retrieves a customer by personal identification number.
        /// </summary>
        /// <param name="personalIdentificationNumber">The personal identification number of the customer.</param>
        /// <returns>The customer data if found, otherwise a 404 Not Found response.</returns>
        /// <response code="200">Returns the customer data.</response>
        group.MapGet("/{personalIdentificationNumber}", async (string personalIdentificationNumber,
            ICustomerService customerService) =>
        {
            var customers = await customerService.GetCustomerByPersonalIdentificationNumberAsync(personalIdentificationNumber);
            return customers.IsSuccess ? Results.Ok(customers.Value) : Results.Problem(customers.Error);
        })
        .WithName("GetCustomerByPersonalIdentificationNumber")
        .Produces<CustomerOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
