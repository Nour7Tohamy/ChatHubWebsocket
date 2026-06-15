using Application.Features.Auth.Login;
using Application.Features.Auth.Register.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.ViewModel;
using System.Security.Claims;

namespace PresentationMVC;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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

            // ✅ SignInAsync عشان ASP.NET Identity يعرف الـ user
            // وبالتالي [Authorize] على الـ MVC pages هيشتغل صح
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId),
                new(ClaimTypes.Name,           result.DisplayName),
                new(ClaimTypes.Email,          result.Email),
                new("displayName",             result.DisplayName),
                // بنحتفظ بالـ JWT token في الـ claim عشان SignalR يستخدمه
                new("jwt_token",               result.Token)
            };

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            // ✅ نرجع الـ token في JSON كمان عشان SignalR في الـ View يستخدمه
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
        catch (Application.Exceptions.ConflictException ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View(model);
        }
        catch (Application.Exceptions.BadRequestException ex)
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
        // ✅ SignOutAsync بيمسح الـ Identity Cookie بشكل صح
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction(nameof(Login));
    }
}