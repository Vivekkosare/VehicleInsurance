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

        //Add the customer to the repository
        var newCustomer = await _customerRepo.AddCustomerAsync(customer);
        if (!newCustomer.IsSuccess)
        {
            _logger.LogError($"Failed to add customer: {newCustomer.Error}");
            return Result<CustomerOutput>.Failure(newCustomer.Error);
        }
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
        if (!iscustomerDeleted.IsSuccess)
        {
            _logger.LogError($"Failed to delete customer with ID {customerId}: {iscustomerDeleted.Error}");
            return Result<bool>.Failure(iscustomerDeleted.Error);
        }
        _logger.LogInformation($"Customer with ID {customerId} deleted successfully.");
        return Result<bool>.Success(iscustomerDeleted.Value);
    }

    public async Task<Result<IEnumerable<CustomerOutput>>> GetAllCustomersAsync()
    {
        var customers = await _customerRepo.GetAllCustomersAsync();
        if (customers == null || !customers.Value.Any() || !customers.IsSuccess)
        {
            _logger.LogInformation("No customers found.");
            return Result<IEnumerable<CustomerOutput>>.Success(Enumerable.Empty<CustomerOutput>());
        }
        _logger.LogInformation($"{customers.Value.Count()} customers retrieved successfully.");
        return Result<IEnumerable<CustomerOutput>>.Success(customers.Value.Select(c => c.ToCustomerOutput()));
    }

    public async Task<Result<CustomerOutput>> GetCustomerByIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogError("Customer ID cannot be empty.");
            return Result<CustomerOutput>.Failure("Customer ID cannot be empty.");
        }
        var customer = await _customerRepo.GetCustomerByIdAsync(customerId);
        if (customer == null || !customer.IsSuccess)
        {
            _logger.LogError($"Customer with ID {customerId} not found.");
            return Result<CustomerOutput>.Failure($"Customer with ID {customerId} not found.");
        }
        return Result<CustomerOutput>.Success(customer.Value.ToCustomerOutput());
    }

    public async Task<Result<CustomerOutput>> GetCustomerByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        if (string.IsNullOrWhiteSpace(personalIdentificationNumber))
        {
            _logger.LogError("Personal identification number cannot be null or empty.");
            return Result<CustomerOutput>.Failure("Personal identification number cannot be null or empty.");
        }
        var customer = await _customerRepo.GetCustomerByPersonalIdentificationNumberAsync(personalIdentificationNumber);
        if (customer == null || !customer.IsSuccess)
        {
            _logger.LogError($"Customer with personal identification number {personalIdentificationNumber} not found.");
            return Result<CustomerOutput>.Failure($"Customer with personal identification number {personalIdentificationNumber} not found.");
        }
        _logger.LogInformation($"Customer with personal identification number {personalIdentificationNumber} retrieved successfully.");
        return Result<CustomerOutput>.Success(customer.Value.ToCustomerOutput());
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
        var isCustomerUpdated = await _customerRepo.UpdateCustomerAsync(customerId, customerEntity);
        if (!isCustomerUpdated.IsSuccess)
        {
            _logger.LogError($"Failed to update customer with ID {customerId}: {isCustomerUpdated.Error}");
            return Result<CustomerOutput>.Failure(isCustomerUpdated.Error);
        }
        _logger.LogInformation($"Customer with ID {customerId} updated successfully.");

        //Retrieve the updated customer from the repository
        var updatedCustomer = await _customerRepo.GetCustomerByIdAsync(customerId);
        if (updatedCustomer == null || !updatedCustomer.IsSuccess)
        {
            _logger.LogError($"Customer with ID {customerId} not found after update.");
            return Result<CustomerOutput>.Failure($"Customer with ID {customerId} not found after update.");
        }
        _logger.LogInformation($"Customer with ID {customerId} retrieved successfully after update.");

        return Result<CustomerOutput>.Success(updatedCustomer.Value.ToCustomerOutput());

    }
}
