using Application.DTOs.MessageDTOs;
using Application.Infrastructure.Services.Messages;
using Application.Infrastructure.Services.Notifitions;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace PresentationMVC;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notificationService;

    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private static readonly object _lock = new();

    public ChatHub(
        IMessageService messageService,
        UserManager<AppUser> userManager,
        INotificationService notificationService)
    {
        _messageService = messageService;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    // =========================
    // CONNECTION
    // =========================
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId is null) return;

        lock (_lock)
        {
            if (!_userConnections.ContainsKey(userId))
                _userConnections[userId] = new HashSet<string>();

            _userConnections[userId].Add(Context.ConnectionId);
        }

        await SetOnlineStatusAsync(userId, true);
        await Clients.Others.SendAsync("UserStatusChanged", userId, true);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId is null) return;

        bool isOffline = false;

        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var conns))
            {
                conns.Remove(Context.ConnectionId);
                if (conns.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                    isOffline = true;
                }
            }
        }

        if (isOffline)
        {
            await SetOnlineStatusAsync(userId, false);
            await Clients.Others.SendAsync("UserStatusChanged", userId, false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // =========================
    // ROOM
    // =========================
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoinedRoom", GetDisplayName());
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeftRoom", GetDisplayName());
    }

    // =========================
    // TYPING
    // =========================
    public async Task StartTypingInRoom(string roomId)
    {
        await Clients.GroupExcept(roomId, Context.ConnectionId)
            .SendAsync("UserTyping", GetUserId(), GetDisplayName(), true, roomId, null);
    }

    public async Task StopTypingInRoom(string roomId)
    {
        await Clients.GroupExcept(roomId, Context.ConnectionId)
            .SendAsync("UserTyping", GetUserId(), GetDisplayName(), false, roomId, null);
    }

    public async Task StartTypingPrivate(string toUserId)
    {
        var conns = GetConnections(toUserId);
        if (conns.Any())
            await Clients.Clients(conns)
                .SendAsync("UserTyping", GetUserId(), GetDisplayName(), true, null, toUserId);
    }

    public async Task StopTypingPrivate(string toUserId)
    {
        var conns = GetConnections(toUserId);
        if (conns.Any())
            await Clients.Clients(conns)
                .SendAsync("UserTyping", GetUserId(), GetDisplayName(), false, null, toUserId);
    }

    // =========================
    // PRIVATE CHAT
    // =========================
    public async Task SendPrivate(string toUserId, string message)
    {
        var senderId = GetUserId();
        if (senderId is null) return;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            Content = message,
            ReceiverId = toUserId,
            Type = MessageType.Text
        });

        await _notificationService.CreateAsync(
            toUserId,
            $"{GetDisplayName()} sent you a message",
            $"/Messages/Private?receiverId={senderId}"
        );

        await NotifyUser(toUserId, new
        {
            senderId,
            senderName = GetDisplayName(),
            preview = Truncate(message, 60),
            url = $"/Messages/Private?receiverId={senderId}"
        });

        await DeliverPrivate(senderId, toUserId, "ReceivePrivate", result);
    }

    // =========================
    // VOICE PRIVATE
    // =========================
    public async Task SendPrivateVoice(string toUserId, string audioUrl)
    {
        var senderId = GetUserId();
        if (senderId is null) return;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            AudioUrl = audioUrl,
            ReceiverId = toUserId,
            Type = MessageType.Voice
        });

        await _notificationService.CreateAsync(
            toUserId,
            $"{GetDisplayName()} sent you a voice message",
            $"/Messages/Private?receiverId={senderId}"
        );

        await NotifyUser(toUserId, new
        {
            senderId,
            senderName = GetDisplayName(),
            preview = "🎤 Voice message",
            url = $"/Messages/Private?receiverId={senderId}"
        });

        await DeliverPrivate(senderId, toUserId, "ReceivePrivate", result);
    }

    // =========================
    // ROOM MESSAGES
    // =========================
    public async Task SendRoomMessage(string roomId, string message)
    {
        var senderId = GetUserId();
        if (senderId is null) return;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            Content = message,
            RoomId = Guid.Parse(roomId),
            Type = MessageType.Text
        });

        // ابعت الرسالة للكل في الـ Room
        await Clients.Group(roomId).SendAsync("ReceiveMessage", result);

        // ابعت notification صوت لكل الناس المتصلين غير المرسل
        var notifPayload = new
        {
            senderId,
            senderName = GetDisplayName(),
            preview = Truncate(message, 60),
            url = $"/Messages/Room?roomId={roomId}"
        };

        List<string> othersConns;
        lock (_lock)
        {
            othersConns = _userConnections
                .Where(kv => kv.Key != senderId)
                .SelectMany(kv => kv.Value)
                .ToList();
        }

        if (othersConns.Any())
            await Clients.Clients(othersConns).SendAsync("ReceiveNotification", notifPayload);
    }

    // =========================
    // VOICE ROOM
    // =========================
    public async Task SendVoiceMessage(string roomId, string audioUrl)
    {
        var senderId = GetUserId();
        if (senderId is null) return;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            AudioUrl = audioUrl,
            RoomId = Guid.Parse(roomId),
            Type = MessageType.Voice
        });

        // ابعت الرسالة للكل في الـ Room
        await Clients.Group(roomId).SendAsync("ReceiveMessage", result);

        // ابعت notification صوت لكل الناس المتصلين غير المرسل
        var notifPayload = new
        {
            senderId,
            senderName = GetDisplayName(),
            preview = "🎤 Voice message",
            url = $"/Messages/Room?roomId={roomId}"
        };

        List<string> othersConns;
        lock (_lock)
        {
            othersConns = _userConnections
                .Where(kv => kv.Key != senderId)
                .SelectMany(kv => kv.Value)
                .ToList();
        }

        if (othersConns.Any())
            await Clients.Clients(othersConns).SendAsync("ReceiveNotification", notifPayload);
    }

    // =========================
    // HELPERS
    // =========================
    private string? GetUserId()
        => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private string GetDisplayName()
        => Context.User?.FindFirstValue("displayName")
           ?? Context.User?.Identity?.Name
           ?? "Unknown";

    private Task DeliverPrivate(string senderId, string receiverId, string eventName, object payload)
    {
        var senderConn = GetConnections(senderId);
        var receiverConn = GetConnections(receiverId);

        return Clients.Clients(senderConn.Concat(receiverConn))
            .SendAsync(eventName, payload);
    }

    private List<string> GetConnections(string userId)
    {
        lock (_lock)
        {
            return _userConnections.TryGetValue(userId, out var conns)
                ? conns.ToList()
                : new List<string>();
        }
    }

    private Task NotifyUser(string userId, object payload)
    {
        var conns = GetConnections(userId);
        return conns.Any()
            ? Clients.Clients(conns).SendAsync("ReceiveNotification", payload)
            : Task.CompletedTask;
    }

    private Task SetOnlineStatusAsync(string userId, bool isOnline)
    {
        return _userManager.FindByIdAsync(userId)
            .ContinueWith(async t =>
            {
                var user = await t;
                if (user == null) return;
                user.IsOnline = isOnline;
                await _userManager.UpdateAsync(user);
            }).Unwrap();
    }

    private static string Truncate(string text, int max)
        => text.Length <= max ? text : text[..max] + "...";
}