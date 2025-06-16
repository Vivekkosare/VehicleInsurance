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
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
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

        // Remove all customers cache to ensure the new customer is included in future requests
        await RemoveAllCustomersCacheAsync();

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
        await _cache.SetAsync(cacheKey, customerExists, _cacheTimeout);
        _logger.LogInformation("Customer existence check for email {Email} completed. Exists: {Exists}", email, customerExists);

        return Result<bool>.Success(customerExists);
    }

    public async Task<Result<bool>> DeleteCustomerAsync(Guid customerId)
    {
        //existing customer
        var customer = await GetCustomerByIdAsync(customerId);

        dbContext.Customers.Remove(customer.Value);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Customer with ID {CustomerId} deleted successfully.", customerId);

        // Remove the customer from cache
        var existingCustomercacheKey = $"Customer_{customerId}";
        await _cache.RemoveAsync(existingCustomercacheKey);
        _logger.LogInformation("Customer with ID {CustomerId} removed from cache.", customerId);

        // Invalidate the customer existence cache
        var existenceCacheKey = $"CustomerExists_{customer.Value.Email}";
        await _cache.RemoveAsync(existenceCacheKey);
        _logger.LogInformation("Invalidated customer existence cache for email {Email}.", customer.Value.Email);

        //Remove all customers cache to ensure the deleted customer is not included in future requests
        await RemoveAllCustomersCacheAsync();
        _logger.LogInformation("Removed all customers cache after deletion for customer ID {CustomerId}.", customerId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<Customer>>> GetAllCustomersAsync()
    {
        var cacheKey = "AllCustomers";
        var cachedCustomers = await _cache.GetAsync<IEnumerable<Customer>>(cacheKey);
        if (cachedCustomers.IsSuccess && cachedCustomers.Value != null)
        {
            _logger.LogInformation("Retrieved all customers from cache.");
            return Result<IEnumerable<Customer>>.Success(cachedCustomers.Value);
        }
        _logger.LogInformation("Cache miss for all customers, retrieving from database.");

        // If not in cache, retrieve from the database
        var customers = await dbContext.Customers.ToListAsync();
        if (customers == null || !customers.Any())
        {
            _logger.LogInformation("No customers found in the database.");
            return Result<IEnumerable<Customer>>.Success(new List<Customer>());
        }
        _logger.LogInformation("Retrieved {Count} customers from the database.", customers.Count);

        // Cache the customers for future requests
        await _cache.SetAsync(cacheKey, customers, _cacheTimeout);
        _logger.LogInformation("Cached all customers for future requests.");

        return Result<IEnumerable<Customer>>.Success(customers);
    }

    public async Task<Result<Customer>> GetCustomerByIdAsync(Guid customerId)
    {
        var cacheKey = $"Customer_{customerId}";
        var customerInCache = await _cache.GetAsync<bool>(cacheKey);
        if (customerInCache.IsSuccess && !customerInCache.Value)
        {
            _logger.LogError($"Customer with ID {customerId} does not exist in cache.");
            return Result<Customer>.Failure($"Customer with ID {customerId} does not exist.");
        }

        // If not in cache, retrieve from the database
        _logger.LogInformation($"Retrieving customer with ID {customerId} from database.");
        var customer = await dbContext.Customers.FindAsync(customerId);
        if (customer == null)
        {
            _logger.LogError($"Customer with ID {customerId} not found.");
            return Result<Customer>.Failure($"Customer with ID {customerId} not found.");
        }
        _logger.LogInformation($"Customer with ID {customerId} retrieved successfully.");

        // Cache the customer for future requests
        await _cache.SetAsync(cacheKey, customer, _cacheTimeout);
        _logger.LogInformation($"Cached customer with ID {customerId} for future requests.");

        return Result<Customer>.Success(customer);
    }

    public async Task<Result<Customer>> GetCustomerByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
    {
        var cacheKey = $"CustomerByPIN_{personalIdentificationNumber}";
        var customerInCache = await _cache.GetAsync<bool>(cacheKey);
        if (customerInCache.IsSuccess && !customerInCache.Value)
        {
            _logger.LogError($"Customer with personalIdentificationNumber {personalIdentificationNumber} does not exist in cache.");
            return Result<Customer>.Failure($"Customer with personalIdentificationNumber {personalIdentificationNumber} does not exist.");
        }

        // If not in cache, retrieve from the database
        _logger.LogInformation($"Retrieving customer with personalIdentificationNumber {personalIdentificationNumber} from database.");
        var customer = await dbContext.Customers
                        .FirstOrDefaultAsync(c => c.PersonalIdentificationNumber == personalIdentificationNumber);
        if (customer == null)
        {
            _logger.LogError($"Customer with personalIdentificationNumber {personalIdentificationNumber} not found.");
            return Result<Customer>.Failure($"Customer with personalIdentificationNumber {personalIdentificationNumber} not found.");
        }
        _logger.LogInformation($"Customer with personalIdentificationNumber {personalIdentificationNumber} retrieved successfully.");

        // Cache the customer for future requests
        await _cache.SetAsync(cacheKey, customer, _cacheTimeout);
        _logger.LogInformation($"Cached customer with personalIdentificationNumber {personalIdentificationNumber} for future requests.");

        return Result<Customer>.Success(customer);
    }


    public async Task<Result<bool>> UpdateCustomerAsync(Guid customerId, Customer customer)
    {
        var existingCustomer = await GetCustomerByIdAsync(customerId);
        if (!existingCustomer.IsSuccess && existingCustomer.Value == null)
        {
            _logger.LogError($"Customer with ID {customerId} not found for update.");
            return Result<bool>.Failure($"Customer with ID {customerId} not found.");
        }

        //Update the existing customer with the new values
        _logger.LogInformation($"Updating customer with ID {customerId}.");

        dbContext.Entry(existingCustomer.Value).CurrentValues.SetValues(customer);
        dbContext.Customers.Update(existingCustomer.Value);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Customer with ID {customerId} updated successfully.");

        // Update the customer in cache
        var cacheKey = $"Customer_{customerId}";
        await _cache.SetAsync(cacheKey, existingCustomer.Value, _cacheTimeout);
        _logger.LogInformation($"Cached updated customer with ID {customerId} for future requests.");

        // Invalidate the customer existence cache
        var existenceCacheKey = $"CustomerExists_{existingCustomer.Value.Email}";
        await _cache.RemoveAsync(existenceCacheKey);
        _logger.LogInformation($"Invalidated customer existence cache for email {existingCustomer.Value.Email}.");

        // Invalidate the customer existence cache
        await RemoveAllCustomersCacheAsync();
        _logger.LogInformation($"Removed all customers cache after update for customer ID {customerId}.");

        return Result<bool>.Success(true);

    }

    /// <summary>
    /// Removes all customers from the cache.
    /// </summary>
    /// <returns></returns>
    private async Task RemoveAllCustomersCacheAsync()
    {
        var allCustomersCacheKey = "AllCustomers";
        await _cache.RemoveAsync(allCustomersCacheKey);
        _logger.LogInformation("Removed all customers cache.");
    }
}
