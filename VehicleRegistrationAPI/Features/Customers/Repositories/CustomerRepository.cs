using Microsoft.EntityFrameworkCore;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Features.Customers.Repositories;

public class CustomerRepository(VehicleRegistrationDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        if (customer == null)
        {
            throw new ArgumentNullException(nameof(customer));
        }
        
        var newCustomer = await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();
        return newCustomer.Entity;
    }

    public async Task<bool> CustomerExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        }

        return await dbContext.Customers.AnyAsync(c => c.Email == email);
    }

    public async Task DeleteCustomerAsync(Guid customerId)
    {
        var customer = await dbContext.Customers.FindAsync(customerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync();
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
