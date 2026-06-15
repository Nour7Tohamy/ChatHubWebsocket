using Domain.Entities.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var users = await _userManager.Users
            .Where(u => u.Id != currentUserId)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.DisplayName,
                u.IsOnline
            })
            .ToListAsync();

        return Ok(users);
    }
}