using FluentValidation;
using Identity.Application.Auth.Dtos;

namespace Identity.Application.Auth.Validators;

public sealed class SendOtpValidator : AbstractValidator<SendOtpRequestDto>
{
    public SendOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}

public sealed class VerifyOtpValidator : AbstractValidator<VerifyOtpRequestDto>
{
    public VerifyOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]{6}$")
            .WithMessage("کد OTP باید ۶ رقم عددی باشد.");
    }
}
