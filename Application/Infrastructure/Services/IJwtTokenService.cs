using Domain.Entities.Main;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Infrastructure.Services
{
    // Application/Interfaces/IJwtTokenService.cs
    public interface IJwtTokenService
    {
        string GenerateToken(AppUser user);
    }
}
