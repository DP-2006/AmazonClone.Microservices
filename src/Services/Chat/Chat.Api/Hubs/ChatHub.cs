using System.Security.Claims;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly ChatDbContext _db;
    public ChatHub(ChatDbContext db) => _db = db;

    private Guid UserId => Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => Context.User!.FindFirstValue(ClaimTypes.Name) ?? "کاربر";
    private bool IsAdmin => Context.User!.IsInRole("Admin");

    public override async Task OnConnectedAsync()
    {
        // اتصال به گروه شخصی کاربر
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{UserId}");

        // اتصال ادمین به گروه نظارت
        if (IsAdmin)
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

        await base.OnConnectedAsync();
    }

    public async Task JoinRoom(Guid roomId)
    {
        var room = await _db.ChatRooms.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room is null) throw new HubException("اتاق پیدا نشد");
        if (!room.IsParticipant(UserId) && !IsAdmin)
            throw new HubException("دسترسی ندارید");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
    }

    public async Task LeaveRoom(Guid roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
    }

    public async Task Typing(Guid roomId)
    {
        var room = await _db.ChatRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roomId);
        if (room is null) return;

        await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserTyping", new
        {
            roomId,
            userId = UserId,
            userName = UserName
        });

        // ادمین‌ها هم ببینن
        await Clients.Group("admins").SendAsync("UserTyping", new
        {
            roomId, userId = UserId, userName = UserName
        });
    }

    public async Task MarkRead(Guid messageId)
    {
        var msg = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (msg is null || msg.SenderId == UserId) return;

        msg.MarkRead();
        await _db.SaveChangesAsync();

        await Clients.Group($"room_{msg.RoomId}").SendAsync("MessageRead", new
        {
            messageId,
            readAt = msg.ReadAt
        });
    }
}
