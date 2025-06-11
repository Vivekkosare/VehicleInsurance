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
            return customer is not null ? Results.Ok(customer) : Results.NotFound();
        })
        .WithName("GetCustomerById")
        .Produces<CustomerOutput>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <returns>A list of all customers.</returns>
        /// <response code="200">Returns a list of all customers.</response>
        group.MapGet("/", async (ICustomerService customerService) =>
        {
            var customers = await customerService.GetAllCustomersAsync();
            return Results.Ok(customers);
        })
        .WithName("GetAllCustomers")
        .Produces<IEnumerable<CustomerOutput>>(StatusCodes.Status200OK);


        /// <summary>
        /// Adds a new customer.
        /// </summary>
        /// <param name="customerInput">The customer data to add.</param>
        /// <returns>The created customer data.</returns>
        /// <response code="201">Returns the created customer data.</response>
        /// <response code="400">If the input data is invalid.</response>
        group.MapPost("/add", async (CustomerInput customerInput, ICustomerService customerService) =>
        {
            var newCustomer = await customerService.AddCustomerAsync(customerInput);
            return Results.Created($"/{newCustomer.Id}", newCustomer);
        })
        .WithName("AddCustomer")
        .Accepts<CustomerInput>("application/json")
        .Produces<CustomerOutput>(StatusCodes.Status201Created);

        /// <summary>
        /// Updates an existing customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to update.</param>
        /// <param name="customerInput">The updated customer data.</param>
        /// <returns>The updated customer data.</returns>
        /// <response code="200">Returns the updated customer data.</response>
        /// <response code="404">If the customer with the specified ID does not exist.</response>
        group.MapPut("/{id:guid}", async (Guid id, CustomerInput customerInput, ICustomerService customerService) =>
        {
            var updatedCustomer = await customerService.UpdateCustomerAsync(id, customerInput);
            return Results.Ok(updatedCustomer);
        })
        .WithName("UpdateCustomer")
        .Accepts<CustomerInput>("application/json")
        .Produces(StatusCodes.Status200OK);

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to delete.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the customer was successfully deleted.</response>
        /// <response code="404">If the customer with the specified ID does not exist.</response>
        group.MapDelete("/{id:guid}", async (Guid id, ICustomerService customerService) =>
        {
            await customerService.DeleteCustomerAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteCustomer")
        .Produces(StatusCodes.Status204NoContent);
    }
}
