using Application.DTOs.RoomDTOs;
using Application.Features.Rooms.Commands.Create;
using Application.Features.Rooms.Commands.Delete;
using Application.Features.Rooms.Commands.Join;
using Application.Features.Rooms.Commands.Leave;
using Application.Features.Rooms.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.ViewModel;
using System.Security.Claims;

namespace PresentationMVC.Controllers;

[Authorize]
public class RoomsController : Controller
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var rooms = await _mediator.Send(new GetAllRoomsQuery());

        return View(rooms.Select(r => new RoomViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            MemberCount = r.MembersCount
        }));
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateRoomViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoomViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var room = await _mediator.Send(new CreateRoomCommand(new CreateRoomDto
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty
            }, UserId));

            return RedirectToAction("Room", "Messages", new { roomId = room.Id });
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id)
    {
        try
        {
            await _mediator.Send(new JoinRoomCommand(id, UserId));
            return RedirectToAction("Room", "Messages", new { roomId = id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(Guid id)
    {
        try
        {
            await _mediator.Send(new LeaveRoomCommand(id, UserId));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteRoomCommand(id, UserId));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}