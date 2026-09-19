using Orders.Domain.Enums;

namespace Orders.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public OrderAddress? Address { get; private set; }
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    private Order() { }

    public Order(Guid userId, string orderNumber, string currency)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId required.");
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("OrderNumber required.");
        UserId = userId;
        OrderNumber = orderNumber.Trim();
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.ToUpperInvariant();
    }

    public void AddItem(Guid productId, Guid? variantId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (unitPrice < 0) throw new ArgumentException("Price cannot be negative.");
        Items.Add(new OrderItem(Id, productId, variantId, productName, unitPrice, quantity));
        RecalculateTotal();
    }

    public void SetAddress(string street, string city, string state, string postalCode, string country)
    {
        Address = new OrderAddress(Id, street, city, state, postalCode, country);
    }

    public void Confirm() { Status = OrderStatus.Confirmed; UpdatedAt = DateTime.UtcNow; }
    public void MarkPaid() { Status = OrderStatus.Paid; UpdatedAt = DateTime.UtcNow; }
    public void Ship() { Status = OrderStatus.Shipped; UpdatedAt = DateTime.UtcNow; }
    public void Deliver() { Status = OrderStatus.Delivered; UpdatedAt = DateTime.UtcNow; }
    public void Cancel() { Status = OrderStatus.Cancelled; UpdatedAt = DateTime.UtcNow; }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.UnitPrice * i.Quantity);
        UpdatedAt = DateTime.UtcNow;
    }
}
