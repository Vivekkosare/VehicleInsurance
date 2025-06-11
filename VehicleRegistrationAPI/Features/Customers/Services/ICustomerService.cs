using VehicleRegistrationAPI.Features.Customers.DTOs;

namespace VehicleRegistrationAPI.Features.Customers.Services;

public interface ICustomerService
{
    Task<CustomerOutput> GetCustomerByIdAsync(Guid customerId);
    Task<IEnumerable<CustomerOutput>> GetAllCustomersAsync();
    Task<CustomerOutput> AddCustomerAsync(CustomerInput  customerInput);
    Task<CustomerOutput> UpdateCustomerAsync(Guid customerId, CustomerInput customer);
    Task DeleteCustomerAsync(Guid customerId);
}
