using Infrastructure.Data;

namespace PresentationMVC.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController : AppController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public AdminController(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    // ─── GET /Admin ───────────────────────────────────────────
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.OnlineUsers = await _userManager.Users.CountAsync(u => u.IsOnline);
        ViewBag.TotalRooms = await _db.Rooms.CountAsync();
        ViewBag.TotalMessages = await _db.Messages.CountAsync();

        // recent 5 users
        ViewBag.RecentUsers = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View();
    }

    // ─── GET /Admin/Users ─────────────────────────────────────
    [HttpGet("Users")]
    public async Task<IActionResult> Users(string? search, int page = 1)
    {
        const int pageSize = 20;

        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.DisplayName.Contains(search) ||
                u.Email!.Contains(search) ||
                u.UserName!.Contains(search));

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // fetch roles per user
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in users)
            userRoles[u.Id] = await _userManager.GetRolesAsync(u);

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.UserRoles = userRoles;
        ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

        return View(users);
    }

    // ─── POST /Admin/Users/ChangeRole ─────────────────────────
    [HttpPost("Users/ChangeRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        // prevent self-demotion
        if (userId == UserId)
        {
            TempData["ErrorMessage"] = "You can't change your own role.";
            return RedirectToAction(nameof(Users));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole);

        TempData["SuccessMessage"] = $"{user.DisplayName}'s role changed to {newRole}.";
        return RedirectToAction(nameof(Users));
    }

    // ─── POST /Admin/Users/Delete ─────────────────────────────
    [HttpPost("Users/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (userId == UserId)
        {
            TempData["ErrorMessage"] = "You can't delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        await _userManager.DeleteAsync(user);
        TempData["SuccessMessage"] = $"User {user.DisplayName} deleted.";
        return RedirectToAction(nameof(Users));
    }

    // ─── GET /Admin/Rooms ─────────────────────────────────────
    [HttpGet("Rooms")]
    public async Task<IActionResult> Rooms(string? search, int page = 1)
    {
        const int pageSize = 20;

        var query = _db.Rooms
            .Include(r => r.RoomMembers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.Contains(search));

        var total = await query.CountAsync();
        var rooms = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(rooms);
    }

    // ─── POST /Admin/Rooms/Delete ─────────────────────────────
    [HttpPost("Rooms/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var room = await _db.Rooms.FindAsync(roomId);
        if (room is null) return NotFound();

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Room \"{room.Name}\" deleted.";
        return RedirectToAction(nameof(Rooms));
    }

    // ─── GET /Admin/Messages ──────────────────────────────────
    [HttpGet("Messages")]
    public async Task<IActionResult> Messages(string? search, int page = 1)
    {
        const int pageSize = 30;

        var query = _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Room)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.Content.Contains(search) ||
                m.Sender.DisplayName.Contains(search));

        var total = await query.CountAsync();
        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(messages);
    }

    // ─── POST /Admin/Messages/Delete ─────────────────────────
    [HttpPost("Messages/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var msg = await _db.Messages.FindAsync(messageId);
        if (msg is null) return NotFound();

        _db.Messages.Remove(msg);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Message deleted.";
        return RedirectToAction(nameof(Messages));
    }
}