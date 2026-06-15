using Application.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Infrastructure.Services.Messages
{
    public interface ICookieAuthService
    {
        /// <summary>
        /// بيبني الـ claims من الـ AuthResult وبيعمل SignInAsync.
        /// </summary>
        Task SignInAsync(HttpContext httpContext, AuthResponseDto result);

        /// <summary>
        /// بيعمل SignOutAsync ويمسح الـ Cookie.
        /// </summary>
        Task SignOutAsync(HttpContext httpContext);
    }
}
