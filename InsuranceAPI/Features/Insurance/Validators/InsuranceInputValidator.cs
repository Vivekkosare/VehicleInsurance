using FluentValidation;
using InsuranceAPI.Features.Insurance.DTOs;

namespace InsuranceAPI.Features.Insurance.Validators;

public class InsuranceInputValidator : AbstractValidator<InsuranceInput>
{
    public InsuranceInputValidator()
    {
        RuleFor(x => x.InsuranceProductId)
            .NotEmpty().WithMessage("Insurance product ID is required.");
            
        RuleFor(x => x.PersonalIdentificationNumber)
            .NotEmpty().WithMessage("Personal identification number is required.")
            .Length(6, 20).WithMessage("Personal identification number must be between 6 and 20 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.")
            .Must((input, startDate) => startDate <= input.EndDate)
            .WithMessage("Start date should not be greater than end date.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.InsuredItemIdentity)
            .NotEmpty().WithMessage("InsuredItem must not be empty.")
            .MaximumLength(100).WithMessage("Insured item identity must be at most 100 characters.");
    }
}
