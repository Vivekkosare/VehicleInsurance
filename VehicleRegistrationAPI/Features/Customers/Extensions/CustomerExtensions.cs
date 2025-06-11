using Microsoft.Identity.Client;
using VehicleRegistrationAPI.Entities;
using VehicleRegistrationAPI.Features.Customers.DTOs;

namespace VehicleRegistrationAPI.Features.Customers.Extensions;

public static class CustomerExtensions
{
    public static Customer ToCustomer(this CustomerInput customerInput)
    {
        if (customerInput == null)
        {
            throw new ArgumentNullException(nameof(customerInput));
        }
        return new Customer
        {
            Name = customerInput.Name,
            Email = customerInput.Email,
            PhoneNumber = customerInput.PhoneNumber
        };
    }

    public static CustomerOutput ToCustomerOutput(this Customer customer)
    {
        if (customer == null)
        {
            throw new ArgumentNullException(nameof(customer));
        }
        return new CustomerOutput(customer.Id, customer.Name, customer.Email, customer.PhoneNumber);
    }
}
