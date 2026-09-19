using FluentValidation;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.Validators;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.ProductName).NotEmpty().MaximumLength(250);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.Street).NotEmpty().When(x => x.Address != null);
        RuleFor(x => x.Address.City).NotEmpty().When(x => x.Address != null);
        RuleFor(x => x.Address.Country).NotEmpty().When(x => x.Address != null);
    }
}
