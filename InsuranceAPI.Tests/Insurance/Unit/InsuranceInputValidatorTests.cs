using InsuranceAPI.Features.Insurance.DTOs;
using InsuranceAPI.Features.Insurance.Validators;

namespace InsuranceAPI.Tests.Insurance.Unit;

public class InsuranceInputValidatorTests
{
    private readonly InsuranceInputValidator _validator = new();
    private readonly string _personalIdentificationNumber = "123456";
    private readonly string _carInsuredItem = "Car";


    [Fact]
    public void Should_Fail_When_InsuredItem_Is_Empty()
    {
        // Test with empty string
        var input = new InsuranceInput(Guid.NewGuid(),
                        _personalIdentificationNumber, string.Empty, DateTime.Today,
                        DateTime.Today.AddDays(1));
        var result = _validator.Validate(input);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InsuredItemIdentity");
    }

    [Fact]
    public void Should_Fail_When_StartDate_After_EndDate()
    {
        var input = new InsuranceInput(Guid.NewGuid(), _personalIdentificationNumber,
                    _carInsuredItem, DateTime.Today.AddDays(2), DateTime.Today);
        var result = _validator.Validate(input);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate");
    }

    [Fact]
    public void Should_Pass_With_Valid_Input()
    {
        var input = new InsuranceInput(Guid.NewGuid(), _personalIdentificationNumber,
                    _carInsuredItem, DateTime.Today, DateTime.Today.AddDays(1));
        var result = _validator.Validate(input);
        Assert.True(result.IsValid);
    }
}
