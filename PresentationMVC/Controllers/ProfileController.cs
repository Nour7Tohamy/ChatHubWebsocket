using Domain.Entities.Main;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PresentationMVC.Controllers;

[Authorize]
public class ProfileController : AppController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(
        UserManager<AppUser> userManager,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _db = db;
        _env = env;
    }

    // ─── GET /Profile ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.Users
            .Include(u => u.RoomMembers)
            .Include(u => u.SentMessages)
            .FirstOrDefaultAsync(u => u.Id == UserId);

        if (user is null) return NotFound();

        ViewBag.RoomsCount = user.RoomMembers.Count;
        ViewBag.MessagesCount = user.SentMessages.Count;
        ViewBag.IsAdmin = User.IsInRole("Admin");

        // Admin extras
        if (User.IsInRole("Admin"))
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.OnlineUsers = await _userManager.Users.CountAsync(u => u.IsOnline);
            ViewBag.TotalRooms = await _db.Rooms.CountAsync();
            ViewBag.TotalMessages = await _db.Messages.CountAsync();
        }

        return View(user);
    }

    // ─── POST /Profile/UpdateName ─────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            TempData["ErrorMessage"] = "Display name cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null) return NotFound();

        user.DisplayName = displayName.Trim();
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Display name updated.";
        return RedirectToAction(nameof(Index));
    }

    // ─── POST /Profile/UploadPicture ──────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile picture)
    {
        if (picture is null || picture.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select an image.";
            return RedirectToAction(nameof(Index));
        }

        // 2 MB max
        if (picture.Length > 2 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Image must be under 2 MB.";
            return RedirectToAction(nameof(Index));
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(picture.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
        {
            TempData["ErrorMessage"] = "Only JPG, PNG, or WebP images are allowed.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null) return NotFound();

        // حذف الصورة القديمة لو موجودة
        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            var oldPath = Path.Combine(_env.WebRootPath, user.ProfilePicture.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        // حفظ الصورة الجديدة
        var avatarsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(avatarsDir);

        var fileName = $"{UserId}{ext}";
        var filePath = Path.Combine(avatarsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await picture.CopyToAsync(stream);

        user.ProfilePicture = $"/uploads/avatars/{fileName}";
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Profile picture updated.";
        return RedirectToAction(nameof(Index));
    }
}