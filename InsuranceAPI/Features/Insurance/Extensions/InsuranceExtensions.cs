using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Features.Insurance.Extensions;

public static class InsuranceExtensions
{
    public static InsuranceProductOutput ToOutput(this InsuranceProduct product)
    {
        return new InsuranceProductOutput(
            product.Id,
            product.Name,
            product.Code,
            product.Price
        );
    }

    public static InsuranceInput ToRequest(this InsuranceProduct product, string personalIdentificationNumber,
        string insuredItem, DateTime startDate, DateTime endDate)
    {
        return new InsuranceInput(
            product.Id,
            personalIdentificationNumber,
            insuredItem,
            startDate,
            endDate
        );
    }

    public static InsuranceOutput ToOutput(this Entities.Insurance insurance)
    {
        return new InsuranceOutput(
            insurance.Id,
            insurance.PersonalIdentificationNumber,
            insurance.InsuranceProduct?.ToOutput() ?? new InsuranceProductOutput(Guid.Empty, string.Empty, string.Empty, 0),
            insurance.StartDate,
            insurance.EndDate,
            null
        );
    }

    public static Entities.Insurance ToEntity(this InsuranceInput request)
    {
        return new Entities.Insurance
        {
            InsuranceProductId = request.InsuranceProductId,
            PersonalIdentificationNumber = request.PersonalIdentificationNumber,
            InsuredItem = request.InsuredItem,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
    public static Entities.Insurance ToEntity(this InsuranceInput request, InsuranceProduct product)
    {
        return new Entities.Insurance
        {
            InsuranceProductId = product.Id,
            PersonalIdentificationNumber = request.PersonalIdentificationNumber,
            InsuredItem = request.InsuredItem,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
    public static Entities.Insurance ToEntity(this InsuranceOutput response)
    {
        return new Entities.Insurance
        {
            PersonalIdentificationNumber = response.PersonalIdentificationNumber,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            InsuranceProductId = response.InsuranceProduct.InsuranceProductId,
        };
    }
}
