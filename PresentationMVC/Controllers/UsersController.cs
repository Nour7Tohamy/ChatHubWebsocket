using PresentationMVC.ViewModel;

namespace PresentationMVC.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var users = await _userManager.Users
            .Where(u => u.Id != currentUserId)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email!,
                DisplayName = u.DisplayName ?? u.UserName!,
                IsOnline = u.IsOnline
            })
            .ToListAsync();

        return View(users);
    }
}