using FluentValidation;
using InsuranceAPI.Features.FeatureManagement.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Validators;

public class FeatureToggleNameInputValidator : AbstractValidator<FeatureToggleNameInput>
{
    public FeatureToggleNameInputValidator()
    {
        RuleFor(x => x.Names)
            .NotEmpty().WithMessage("Feature toggle names are required.")
            .Must(names => names.All(name => !string.IsNullOrWhiteSpace(name)))
            .WithMessage("Feature toggle names cannot contain empty or whitespace values.");
    }
}
