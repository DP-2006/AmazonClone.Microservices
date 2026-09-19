namespace Orders.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, Guid? variantId, string productName, decimal unitPrice, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        VariantId = variantId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
