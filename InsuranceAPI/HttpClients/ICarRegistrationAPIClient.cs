using InsuranceAPI.Features.Insurance.DTOs;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.HttpClients;

public interface ICarRegistrationAPIClient
{
    Task<IEnumerable<CarDto>> GetCarRegistrationAsync(string personalIdentificationNumber);
    Task<IEnumerable<CarDto>> GetCarRegistrationsByPersonIdsAsync(PersonIdentifiersRequest personIdsRequest);
    Task<CustomerOutput?> GetCustomerByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
}
