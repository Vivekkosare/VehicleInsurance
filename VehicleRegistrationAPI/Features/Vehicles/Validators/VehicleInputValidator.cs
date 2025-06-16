using FluentValidation;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;

namespace VehicleRegistrationAPI.Features.Vehicles.Validators;

public class VehicleInputValidator : AbstractValidator<VehicleInput>
{
    public VehicleInputValidator()
    {
        RuleFor(v => v.Name).NotEmpty().WithMessage("Vehicle name is required.");
        RuleFor(v => v.RegistrationNumber).NotEmpty().WithMessage("Registration number is required.");
        RuleFor(v => v.Make).NotEmpty().WithMessage("Make is required.");
        RuleFor(v => v.Model).NotEmpty().WithMessage("Model is required.");

        RuleFor(v => v.Year)
            .NotEmpty()
            .InclusiveBetween(1900, DateTime.Now.Year)
            .WithMessage($"Year must be between 1900 and {DateTime.Now.Year}.");

        RuleFor(v => v.Color).NotEmpty().WithMessage("Color is required.");

        RuleFor(v => v.RegistrationDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Registration date cannot be in the future.");
            
        RuleFor(v => v.OwnerId).NotEmpty().WithMessage("OwnerId is required.");
    }
}
