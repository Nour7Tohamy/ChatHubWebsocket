using Application.DTOs.AuthDTOs;
using MediatR;

namespace Application.Features.Auth.Register.Commands
{
    public record RegisterCommand(RegisterDto Dto) : IRequest<AuthResponseDto>;
}
