using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Features.Customers.DTOs;
using VehicleRegistrationAPI.Features.Customers.Extensions;
using VehicleRegistrationAPI.Features.Customers.Repositories;

namespace VehicleRegistrationAPI.Features.Customers.Services;

public class CustomerService(ICustomerRepository _customerRepo,
ILogger<CustomerService> _logger) : ICustomerService
{
    public async Task<Result<CustomerOutput>> AddCustomerAsync(CustomerInput customerInput)
    {
        if (customerInput == null)
        {
            _logger.LogError("Customer input cannot be null.");
            return Result<CustomerOutput>.Failure("Customer input cannot be null.");
        }
        var customer = customerInput.ToCustomer();
        // if (await _customerRepo.CustomerExistsAsync(customer.Email))
        // {
        //     _logger.LogError($"Customer with email {customer.Email} already exists.");
        //     return Result<CustomerOutput>.Failure($"Customer with email {customer.Email} already exists.");
        // }

        //Add the customer to the repository
        var newCustomer = await _customerRepo.AddCustomerAsync(customer);
        return Result<CustomerOutput>.Success(newCustomer.Value.ToCustomerOutput());
    }



    public async Task<Result<bool>> DeleteCustomerAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogError("Customer ID cannot be empty.");
            return Result<bool>.Failure("Customer ID cannot be empty.");
        }
        var iscustomerDeleted = await _customerRepo.DeleteCustomerAsync(customerId);
        return Result<bool>.Success(iscustomerDeleted);
    }

    public async Task<Result<IEnumerable<CustomerOutput>>> GetAllCustomersAsync()
    {
        var customers = await _customerRepo.GetAllCustomersAsync();
        return Result<IEnumerable<CustomerOutput>>.Success(customers.Select(c => c.ToCustomerOutput()));
    }

    public async Task<Result<CustomerOutput>> GetCustomerByIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogError("Customer ID cannot be empty.");
            return Result<CustomerOutput>.Failure("Customer ID cannot be empty.");
        }
        var customer = await _customerRepo.GetCustomerByIdAsync(customerId);
        return Result<CustomerOutput>.Success(customer.ToCustomerOutput());
    }

    public async Task<Result<CustomerOutput>> UpdateCustomerAsync(Guid customerId, CustomerInput customer)
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogError("Customer ID cannot be empty.");
            return Result<CustomerOutput>.Failure("Customer ID cannot be empty.");
        }
        if (customer == null)
        {
            _logger.LogError("Customer input cannot be null.");
            return Result<CustomerOutput>.Failure("Customer input cannot be null.");
        }

        //transform the input DTO to the entity and update the customer
        var customerEntity = customer.ToCustomer();
        await _customerRepo.UpdateCustomerAsync(customerId, customerEntity);
        _logger.LogInformation($"Customer with ID {customerId} updated successfully.");

        //Retrieve the updated customer from the repository
        var updatedCustomer = await _customerRepo.GetCustomerByIdAsync(customerId);
        _logger.LogInformation($"Customer with ID {customerId} retrieved successfully after update.");

        return Result<CustomerOutput>.Success(updatedCustomer.ToCustomerOutput());

    }
}
