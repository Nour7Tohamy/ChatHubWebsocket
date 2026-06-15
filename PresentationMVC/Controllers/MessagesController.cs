using Application.Features.Rooms.Queries.GetById;

namespace PresentationMVC.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private string DisplayName =>
        User.FindFirstValue("displayName")
        ?? User.FindFirstValue(ClaimTypes.Name)
        ?? "Unknown";

    [HttpGet]
    public async Task<IActionResult> Room(Guid roomId)
    {
        var room = await _mediator.Send(new GetRoomByIdQuery(roomId));
        if (room is null)
            return RedirectToAction("Index", "Rooms");

        var messages = await _mediator.Send(new GetRoomMessagesQuery(roomId, 1, 50));

        ViewBag.RoomId = roomId;
        ViewBag.RoomName = room.Name;
        ViewBag.DisplayName = DisplayName;

        return View(messages);
    }

    [HttpGet]
    public async Task<IActionResult> Private(string receiverId, string? receiverName = null)
    {
        var messages = await _mediator.Send(
            new GetPrivateMessagesQuery(UserId, receiverId));

        ViewBag.TargetUserId = receiverId;
        ViewBag.TargetUserName = receiverName ?? "User";
        ViewBag.DisplayName = DisplayName;

        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid? roomId, string? receiverId)
    {
        await _mediator.Send(new DeleteMessageCommand(id, UserId));

        if (roomId.HasValue)
            return RedirectToAction(nameof(Room), new { roomId });

        return RedirectToAction(nameof(Private), new { receiverId });
    }
}