using Infrastructure.Data;

namespace PresentationMVC.Controllers;

[Authorize]
[Route("Notifications")] 
public class NotificationsPageController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationsPageController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        foreach (var item in notifications.Where(x => !x.IsRead))
            item.IsRead = true;

        await _context.SaveChangesAsync();

        return View("~/Views/Notifications/Index.cshtml", notifications);  // ← ده
    }
}