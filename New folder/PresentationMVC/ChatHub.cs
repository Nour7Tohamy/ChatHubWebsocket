using Application.DTOs.MessageDTOs;
using Application.Infrastructure.Services.Messages;
using Domain.Entities.Main;
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

    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _roomMembers = new();
    private static readonly object _lock = new();

    public ChatHub(IMessageService messageService, UserManager<AppUser> userManager)
    {
        _messageService = messageService;
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        bool wasOffline;
        lock (_lock)
        {
            if (!_userConnections.ContainsKey(userId))
                _userConnections[userId] = new HashSet<string>();

            wasOffline = _userConnections[userId].Count == 0;
            _userConnections[userId].Add(Context.ConnectionId);
        }

        // اكتب في الـ DB بس لو ده أول connection (كان offline)
        if (wasOffline)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                user.IsOnline = true;
                await _userManager.UpdateAsync(user);
            }
        }

        await Clients.Others.SendAsync("UserStatusChanged", userId, true);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        bool isOffline;
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
                else
                {
                    isOffline = false;
                }
            }
            else
            {
                isOffline = true;
            }
        }

        // اكتب في الـ DB بس لو مفيش connections تانية
        if (isOffline)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                user.IsOnline = false;
                await _userManager.UpdateAsync(user);
            }

            await Clients.Others.SendAsync("UserStatusChanged", userId, false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        lock (_lock)
        {
            if (!_roomMembers.ContainsKey(roomId))
                _roomMembers[roomId] = new HashSet<string>();
            _roomMembers[roomId].Add(userId);
        }

        var displayName = Context.User?.FindFirstValue("displayName") ?? userId;
        await Clients.Group(roomId).SendAsync("UserJoinedRoom", displayName);
    }

    public async Task LeaveRoom(string roomId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        lock (_lock)
        {
            if (_roomMembers.TryGetValue(roomId, out var members))
                members.Remove(userId);
        }

        var displayName = Context.User?.FindFirstValue("displayName") ?? userId;
        await Clients.Group(roomId).SendAsync("UserLeftRoom", displayName);
    }

    public async Task SendMessage(string roomId, string message)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var result = await _messageService.SendAsync(userId, new SendMessageDto
        {
            Content = message,
            RoomId = Guid.Parse(roomId),
            ReceiverId = null
        });

        await Clients.Group(roomId).SendAsync("ReceiveMessage", result);
    }

    public async Task SendPrivate(string toUserId, string message)
    {
        var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(senderId)) return;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            Content = message,
            RoomId = null,
            ReceiverId = toUserId
        });

        List<string> receiverConns, senderConns;
        lock (_lock)
        {
            receiverConns = _userConnections.TryGetValue(toUserId, out var rc)
                ? rc.ToList() : new();
            senderConns = _userConnections.TryGetValue(senderId, out var sc)
                ? sc.ToList() : new();
        }

        if (senderConns.Any())
            await Clients.Clients(senderConns).SendAsync("ReceivePrivate", result);

        if (receiverConns.Any())
            await Clients.Clients(receiverConns).SendAsync("ReceivePrivate", result);
    }
}