using FluentValidation;
using Identity.Application.Auth.Dtos;

namespace Identity.Application.Auth.Validators;

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
