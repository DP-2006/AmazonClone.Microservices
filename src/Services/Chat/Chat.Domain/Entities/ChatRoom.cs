namespace Chat.Domain.Entities;

public sealed class ChatRoom
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BuyerId { get; private set; }
    public Guid SellerId { get; private set; }
    public bool SellerChatEnabled { get; private set; } = true;
    public DateTime? LastMessageAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    private ChatRoom() { }

    public ChatRoom(Guid buyerId, Guid sellerId)
    {
        if (buyerId == Guid.Empty) throw new ArgumentException("BuyerId required.");
        if (sellerId == Guid.Empty) throw new ArgumentException("SellerId required.");
        if (buyerId == sellerId) throw new ArgumentException("Buyer and Seller cannot be same.");
        BuyerId = buyerId;
        SellerId = sellerId;
    }

    public void TouchMessage() { LastMessageAt = DateTime.UtcNow; }
    public void DisableChat() { SellerChatEnabled = false; }
    public void EnableChat() { SellerChatEnabled = true; }
    public bool IsParticipant(Guid userId) => userId == BuyerId || userId == SellerId;
}
