using Application.DTOs.MessageDTOs;
using Application.Features.Messages.Commands.Delete;
using Application.Features.Messages.Commands.Send;
using Application.Features.Messages.Queries.Get;
using Application.Features.Messages.Queries.GetPrivate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
    {
        var result = await _mediator.Send(new SendMessageCommand(dto, UserId));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteMessageCommand(id, UserId));
        return Ok(result);
    }

    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetRoomMessages(Guid roomId)
    {
        var result = await _mediator.Send(new GetRoomMessagesQuery(roomId, 1, 20));
        return Ok(result);
    }

    [HttpGet("private/{receiverId}")]
    public async Task<IActionResult> GetPrivateMessages(string receiverId)
    {
        var result = await _mediator.Send(new GetPrivateMessagesQuery(UserId, receiverId));
        return Ok(result);
    }
}