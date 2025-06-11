using VehicleRegistrationAPI.Features.Customers.DTOs;
using VehicleRegistrationAPI.Features.Customers.Extensions;
using VehicleRegistrationAPI.Features.Customers.Repositories;

namespace VehicleRegistrationAPI.Features.Customers.Services;

public class CustomerService(ICustomerRepository _customerRepo) : ICustomerService
{
    public async Task<CustomerOutput> AddCustomerAsync(CustomerInput customerInput)
    {
        if (customerInput == null)
        {
            throw new ArgumentNullException(nameof(customerInput));
        }
        var customer = customerInput.ToCustomer();
        if (await _customerRepo.CustomerExistsAsync(customer.Email))
        {
            throw new InvalidOperationException($"Customer with email {customer.Email} already exists.");
        }
        var newCustomer = await _customerRepo.AddCustomerAsync(customer);
        return newCustomer.ToCustomerOutput();   
    }
    
   

    public Task DeleteCustomerAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }
        return _customerRepo.DeleteCustomerAsync(customerId);
    }

    public async Task<IEnumerable<CustomerOutput>> GetAllCustomersAsync()
    {
        var customers = await _customerRepo.GetAllCustomersAsync();
        return customers.Select(c => c.ToCustomerOutput());
    }

    public async Task<CustomerOutput> GetCustomerByIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }
        var customer = await _customerRepo.GetCustomerByIdAsync(customerId);
        return customer.ToCustomerOutput();
    }

    public async Task<CustomerOutput> UpdateCustomerAsync(Guid customerId, CustomerInput customer)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }
        if (customer == null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        var customerEntity = customer.ToCustomer();
        await _customerRepo.UpdateCustomerAsync(customerId, customerEntity);

        var updatedCustomer = await _customerRepo.GetCustomerByIdAsync(customerId);
        return updatedCustomer.ToCustomerOutput();
        
    }
}
