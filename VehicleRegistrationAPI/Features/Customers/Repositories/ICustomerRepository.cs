using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Features.Customers.Repositories;

public interface ICustomerRepository
{
    Task<Result<Customer>> GetCustomerByIdAsync(Guid customerId);
    Task<Result<IEnumerable<Customer>>> GetAllCustomersAsync();
    Task<Result<Customer>> AddCustomerAsync(Customer customer);
    Task<Result<bool>> UpdateCustomerAsync(Guid customerId, Customer customer);
    Task<Result<bool>> DeleteCustomerAsync(Guid customerId);
    Task<Result<bool>> CustomerExistsAsync(string email);
}
