using Orders.Domain.Enums;

namespace Orders.Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string TransactionId { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Payment() { }

    public Payment(Guid orderId, string provider, string transactionId, decimal amount)
    {
        OrderId = orderId;
        Provider = provider;
        TransactionId = transactionId;
        Amount = amount;
    }

    public void MarkSucceeded() { Status = PaymentStatus.Succeeded; }
    public void MarkFailed() { Status = PaymentStatus.Failed; }
    public void Refund() { Status = PaymentStatus.Refunded; }
}
