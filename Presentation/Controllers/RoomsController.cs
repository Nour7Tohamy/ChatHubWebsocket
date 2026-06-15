using Application.DTOs.RoomDTOs;
using Application.Features.Rooms.Commands.Create;
using Application.Features.Rooms.Commands.Delete;
using Application.Features.Rooms.Commands.Join;
using Application.Features.Rooms.Commands.Leave;
using Application.Features.Rooms.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _mediator.Send(new GetAllRoomsQuery()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomDto dto)
    {
        return Ok(await _mediator.Send(new CreateRoomCommand(dto, UserId)));
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(Guid id)
    {
        return Ok(await _mediator.Send(new JoinRoomCommand(id, UserId)));
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> Leave(Guid id)
    {
        return Ok(await _mediator.Send(new LeaveRoomCommand(id, UserId)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return Ok(await _mediator.Send(new DeleteRoomCommand(id, UserId)));
    }
}