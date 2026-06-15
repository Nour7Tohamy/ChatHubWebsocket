using Application.DTOs.MessageDTOs;
using Application.Features.Messages.Commands.Send;
using Application.Infrastructure.Services.Messages;
using Domain.Entities.Main;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Presentation.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly UserManager<AppUser> _userManager;

    public ChatHub(
        IMessageService messageService,
        UserManager<AppUser> userManager)
    {
        _messageService = messageService;
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.IsOnline = true;
                await _userManager.UpdateAsync(user);

                await Clients.All.SendAsync("UserStatusChanged", new
                {
                    userId,
                    isOnline = true
                });
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendMessage(string roomId, string message)
    {
        var senderId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            Content = message,
            RoomId = Guid.Parse(roomId),
            ReceiverId = null
        });

        await Clients.Group(roomId).SendAsync("ReceiveMessage", result);
    }

    public async Task SendPrivate(string toUserId, string message)
    {
        var senderId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messageService.SendAsync(senderId, new SendMessageDto
        {
            Content = message,
            RoomId = null,
            ReceiverId = toUserId
        });

        await Clients.User(toUserId).SendAsync("ReceivePrivate", result);
        await Clients.User(senderId).SendAsync("ReceivePrivate", result);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.IsOnline = false;
                await _userManager.UpdateAsync(user);

                await Clients.All.SendAsync("UserStatusChanged", new
                {
                    userId,
                    isOnline = false
                });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}