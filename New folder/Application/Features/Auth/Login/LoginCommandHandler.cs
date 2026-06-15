using Application.DTOs.AuthDTOs;
using Application.Exceptions;
using Application.Features.Auth.Login;
using Application.Infrastructure.Services;
using Domain.Entities.Main;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Dto.Email);

        if (user is null)
            throw new UnauthorizedException("Invalid email or password");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Dto.Password);

        if (!isPasswordValid)
            throw new UnauthorizedException("Invalid email or password");

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            DisplayName = user.DisplayName ?? user.UserName!,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }
}