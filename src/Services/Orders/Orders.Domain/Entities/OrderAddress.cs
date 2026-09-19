namespace Orders.Domain.Entities;

public sealed class OrderAddress
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private OrderAddress() { }

    public OrderAddress(Guid orderId, string street, string city, string state, string postalCode, string country)
    {
        OrderId = orderId;
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
