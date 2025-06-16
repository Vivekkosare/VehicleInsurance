using FluentValidation;
using VehicleRegistrationAPI.Features.Customers.DTOs;

namespace VehicleRegistrationAPI.Features.Customers.Validators;

public class CustomerInputValidator : AbstractValidator<CustomerInput>
{
    public CustomerInputValidator()
    {
        RuleFor(c => c.Name).NotEmpty().WithMessage("Customer name is required.");
        RuleFor(c => c.PersonalIdentificationNumber)
            .NotEmpty().WithMessage("Personal identification number is required.")
            .Length(6, 20).WithMessage("Personal identification number must be between 6 and 20 characters.");
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");
        RuleFor(c => c.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Length(7, 20).WithMessage("Phone number must be between 7 and 20 characters.");
    }
}
