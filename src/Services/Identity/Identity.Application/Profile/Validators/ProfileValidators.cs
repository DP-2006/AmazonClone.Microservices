using FluentValidation;
using Identity.Application.Profile.Dtos;

namespace Identity.Application.Profile.Validators;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PostalCode));
        RuleFor(x => x.Address).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Address));
        RuleFor(x => x.City).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.City));
        RuleFor(x => x.State).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.State));
        RuleFor(x => x.Country).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Country));
    }
}

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public sealed class ChangeEmailValidator : AbstractValidator<ChangeEmailDto>
{
    public ChangeEmailValidator()
    {
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class BecomeSellerValidator : AbstractValidator<BecomeSellerDto>
{
    public BecomeSellerValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessType).NotEmpty()
            .Must(x => x == "Product" || x == "Scientific")
            .WithMessage("BusinessType must be 'Product' or 'Scientific'");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Website).MaximumLength(300).When(x => !string.IsNullOrEmpty(x.Website));
    }
}
