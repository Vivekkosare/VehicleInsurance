using FluentValidation;
using InsuranceAPI.Features.FeatureManagement.DTOs;

namespace InsuranceAPI.Features.FeatureManagement.Validators
{
    public class FeatureToggleInputValidator : AbstractValidator<FeatureToggleInput>
    {
        public FeatureToggleInputValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Feature toggle name is required.")
                .MaximumLength(100).WithMessage("Feature toggle name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Feature toggle description is required.")
                .MaximumLength(200).WithMessage("Feature toggle description cannot exceed 200 characters.");

            RuleFor(x => x.IsEnabled)
                .NotNull().WithMessage("Feature toggle enabled status is required.");
        }
    }
}