using Application.DTOs.AuthDTOs;
using Application.Exceptions;
using Application.Infrastructure.Services;
using Domain.Entities.Main;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Register.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Dto.Email);

        if (existingUser is not null)
            throw new ConflictException("Email already exists");

        var user = new AppUser
        {
            UserName = request.Dto.Email,
            Email = request.Dto.Email,
            DisplayName = request.Dto.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Dto.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description
                        ?? "Registration failed";
            throw new BadRequestException(error);
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            DisplayName = user.DisplayName ?? user.Email!,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }
}