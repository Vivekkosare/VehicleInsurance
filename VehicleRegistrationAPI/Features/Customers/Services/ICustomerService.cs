using VehicleInsurance.Shared.DTOs;
using VehicleRegistrationAPI.Features.Customers.DTOs;

namespace VehicleRegistrationAPI.Features.Customers.Services;

public interface ICustomerService
{
    Task<Result<CustomerOutput>> GetCustomerByIdAsync(Guid customerId);
    Task<Result<IEnumerable<CustomerOutput>>> GetAllCustomersAsync();
    Task<Result<CustomerOutput>> AddCustomerAsync(CustomerInput customerInput);
    Task<Result<CustomerOutput>> UpdateCustomerAsync(Guid customerId, CustomerInput customer);
    Task<Result<bool>> DeleteCustomerAsync(Guid customerId);
    Task<Result<CustomerOutput>> GetCustomerByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
}
