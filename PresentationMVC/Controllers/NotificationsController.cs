using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace PresentationMVC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔥 unread count
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var count = await _context.Notifications
                .CountAsync(x => x.UserId == userId && !x.IsRead);

            return Ok(count);
        }

        // 🔥 latest notifications
        [HttpGet("latest")]
        public async Task<IActionResult> Latest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var data = await _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(20)
                .Select(x => new
                {
                    x.Title,
                    x.Url,
                    x.IsRead,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // 🔥 mark all read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _context.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToListAsync();

            foreach (var n in list)
                n.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
