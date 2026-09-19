using Catalog.Application.Products.Dtos;
using FluentValidation;

namespace Catalog.Application.Products.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(250).Matches("^[a-z0-9-]+$");
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
