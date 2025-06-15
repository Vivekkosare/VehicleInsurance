using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Shared.DTOs;
using VehicleInsurance.Shared.Services;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Features.Customers.Repositories;

public class CustomerRepository(VehicleRegistrationDbContext dbContext,
ILogger<CustomerRepository> _logger,
ICacheService _cache) : ICustomerRepository
{
    public async Task<Result<Customer>> AddCustomerAsync(Customer customer)
    {
        if (customer == null)
        {
            _logger.LogError("Customer cannot be null.");
            return Result<Customer>.Failure("Customer cannot be null.");
        }
        //check if the customer already exists
        var customerExists = await CustomerExistsAsync(customer.Email);
        if (customerExists.Value)
        {
            _logger.LogError($"Customer with email {customer.Email} already exists.");
            return Result<Customer>.Failure($"Customer with email {customer.Email} already exists.");
        }
        //Add the customer to the database
        var newCustomer = await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Customer with ID {CustomerId} created successfully.", newCustomer.Entity.Id);

        return Result<Customer>.Success(newCustomer.Entity);
    }

    public async Task<Result<bool>> CustomerExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogError("Email cannot be null or empty.");
            return Result<bool>.Failure("Email cannot be null or empty.");
        }
        var cacheKey = $"CustomerExists_{email}";
        var customerExistsInCache = await _cache.GetAsync<bool>(cacheKey);
        if (customerExistsInCache.IsSuccess)
        {
            _logger.LogInformation("Customer existence check for email {Email} found in cache: {Exists}", email, customerExistsInCache.Value);
            return Result<bool>.Success(customerExistsInCache.Value);
        }
        //Check if the customer exists in the database
        bool customerExists = await dbContext.Customers.AnyAsync(c => c.Email == email);

        // Store the result in cache
        await _cache.SetAsync(cacheKey, customerExists, TimeSpan.FromMinutes(30));
        _logger.LogInformation("Customer existence check for email {Email} completed. Exists: {Exists}", email, customerExists);

        return Result<bool>.Success(customerExists);
    }

    public async Task<Result<bool>> DeleteCustomerAsync(Guid customerId)
    {
        var cacheKey = $"Customer_{customerId}";
        var customerExistsInCache = await _cache.GetAsync<bool>(cacheKey);
        if (customerExistsInCache.IsSuccess && !customerExistsInCache.Value)
        {
            _logger.LogError($"Customer with ID {customerId} does not exist in cache.");
            return Result<bool>.Failure($"Customer with ID {customerId} does not exist.");
        }
        var customer = await dbContext.Customers.FindAsync(customerId);
        if (customer == null)
        {
            _logger.LogError($"Customer with ID {customerId} not found.");
            return Result<bool>.Failure($"Customer with ID {customerId} not found.");
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Customer with ID {CustomerId} deleted successfully.", customerId);

        // Remove the customer from cache
        await _cache.RemoveAsync(cacheKey);
        _logger.LogInformation("Customer with ID {CustomerId} removed from cache.", customerId);

        // Invalidate the customer existence cache
        var existenceCacheKey = $"CustomerExists_{customer.Email}";
        await _cache.RemoveAsync(existenceCacheKey);

        return Result<bool>.Success(true);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        var customers = await dbContext.Customers.ToListAsync();
        return customers ?? new List<Customer>();
    }

    public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
        }
        return customer;
    }

    public async Task<Customer> GetCustomerByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        }

        var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with email {email} not found.");
        }
        return customer;
    }

    public async Task UpdateCustomerAsync(Guid customerId, Customer customer)
    {
        var existingCustomer = await dbContext.Customers.FindAsync(customerId);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
        }
        dbContext.Entry(existingCustomer).CurrentValues.SetValues(customer);
        dbContext.Customers.Update(existingCustomer);
        await dbContext.SaveChangesAsync();

    }
}
