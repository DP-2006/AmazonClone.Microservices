namespace Basket.Domain.Entities;

public sealed class ShoppingBasket
{
    public Guid UserId { get; set; }
    public List<BasketItem> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}
