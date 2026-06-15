using Application.DTOs.AuthDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Login
{
    public record LoginCommand(LoginDto Dto) : IRequest<AuthResponseDto>;
}
