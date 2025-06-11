using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Features.Customers.Repositories;

public interface ICustomerRepository
{
    Task<Customer> GetCustomerByIdAsync(Guid customerId);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer> AddCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Guid customerId, Customer customer);
    Task DeleteCustomerAsync(Guid customerId);
    Task<bool> CustomerExistsAsync(string email);
}
