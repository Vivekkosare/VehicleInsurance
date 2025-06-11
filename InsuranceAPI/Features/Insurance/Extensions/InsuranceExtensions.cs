using InsuranceAPI.DTOs;
using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Features.Insurance.Extensions;

public static class InsuranceExtensions
{
    public static InsuranceProductResponse ToResponse(this InsuranceProduct product)
    {
        return new InsuranceProductResponse(
            product.Id,
            product.Name,
            product.Code,
            product.Price);
    }

    public static InsuranceRequest ToRequest(this InsuranceProduct product, string personalIdentificationNumber,
        DateTime startDate, DateTime endDate)
    {
        return new InsuranceRequest(
            product.Id,
            personalIdentificationNumber,
            startDate,
            endDate);
    }
    public static InsuranceResponse ToResponse(this Entities.Insurance insurance)
    {
        return new InsuranceResponse(
            insurance.PersonalIdentificationNumber,
            insurance.InsuranceProduct.ToResponse(),
            insurance.StartDate,
            insurance.EndDate, null);
    }
    public static Entities.Insurance ToEntity(this InsuranceRequest request)
    {
        return new Entities.Insurance
        {
            InsuranceProductId = request.InsuranceProductId,
            PersonalIdentificationNumber = request.PersonalIdentificationNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
    public static Entities.Insurance ToEntity(this InsuranceResponse response)
    {
        return new Entities.Insurance
        {
            PersonalIdentificationNumber = response.PersonalIdentificationNumber,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            InsuranceProductId = response.InsuranceProduct.InsuranceProductId
        };
    }
    public static Entities.Insurance ToEntity(this InsuranceRequest request, InsuranceProduct product)
    {
        return new Entities.Insurance
        {
            InsuranceProductId = product.Id,
            PersonalIdentificationNumber = request.PersonalIdentificationNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
}
