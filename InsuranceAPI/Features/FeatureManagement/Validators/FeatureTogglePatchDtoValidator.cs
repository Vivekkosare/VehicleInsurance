using FluentValidation;
using InsuranceAPI.Features.FeatureManagement.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Validators;

public class FeatureTogglePatchDtoConfiguration: AbstractValidator<FeatureTogglePatchDto>
{
    public FeatureTogglePatchDtoConfiguration()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.IsEnabled)
            .NotNull()
            .WithMessage("IsEnabled must be specified.");
    }
}
