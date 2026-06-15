using Application.Exceptions;
using Application.Features.Auth.Login;
using Application.Features.Auth.Register.Commands;
using Application.Infrastructure.Services.Messages;
using PresentationMVC.ViewModel;

namespace PresentationMVC.Controllers;

[AllowAnonymous]
public class AuthController : AppController
{
    private readonly IMediator _mediator;
    private readonly ICookieAuthService _cookieAuth;

    public AuthController(IMediator mediator, ICookieAuthService cookieAuth)
    {
        _mediator = mediator;
        _cookieAuth = cookieAuth;
    }

    // ════════════════════════════════════════
    //  GET /Auth/Login
    // ════════════════════════════════════════
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Rooms");

        return View();
    }

    // ════════════════════════════════════════
    //  POST /Auth/Login
    // ════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid data" });

        try
        {
            var result = await _mediator.Send(new LoginCommand(
                new Application.DTOs.AuthDTOs.LoginDto
                {
                    Email = model.Email,
                    Password = model.Password
                }));

            // بناء الـ Cookie تم نقله لـ CookieAuthService
            await _cookieAuth.SignInAsync(HttpContext, result);

            return Json(new
            {
                token = result.Token,
                userId = result.UserId,
                displayName = result.DisplayName
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ════════════════════════════════════════
    //  GET /Auth/Register
    // ════════════════════════════════════════
    [HttpGet]
    public IActionResult Register() => View();

    // ════════════════════════════════════════
    //  POST /Auth/Register
    // ════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _mediator.Send(new RegisterCommand(
                new Application.DTOs.AuthDTOs.RegisterDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    DisplayName = model.DisplayName
                }));

            TempData["SuccessMessage"] = "Account created! Please sign in.";
            return RedirectToAction(nameof(Login));
        }
        catch (ConflictException ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View(model);
        }
        catch (BadRequestException ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    // ════════════════════════════════════════
    //  POST /Auth/Logout
    // ════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _cookieAuth.SignOutAsync(HttpContext);
        return RedirectToAction(nameof(Login));
    }
}