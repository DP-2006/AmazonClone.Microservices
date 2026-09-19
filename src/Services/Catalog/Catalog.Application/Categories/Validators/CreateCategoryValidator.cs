using Catalog.Application.Categories.Dtos;
using FluentValidation;

namespace Catalog.Application.Categories.Validators;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(150).Matches("^[a-z0-9-]+$");
    }
}
