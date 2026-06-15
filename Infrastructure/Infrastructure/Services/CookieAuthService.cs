using Application.DTOs.AuthDTOs;
using Application.Infrastructure.Services.Messages;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Infrastructure.Infrastructure.Services
{
    public class CookieAuthService : ICookieAuthService
    {
        private readonly UserManager<AppUser> _userManager;

        public CookieAuthService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task SignInAsync(HttpContext httpContext, AuthResponseDto result)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId),
                new(ClaimTypes.Name,           result.DisplayName),
                new(ClaimTypes.Email,          result.Email),
                new("displayName",             result.DisplayName),
                new("jwt_token",               result.Token)
            };

            // ← أضف الـ roles في الـ claims
            var user = await _userManager.FindByIdAsync(result.UserId);
            if (user is not null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
        }

        public async Task SignOutAsync(HttpContext httpContext)
            => await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}