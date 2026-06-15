using Application.DTOs.RoomDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rooms.Commands.Create
{
    public record CreateRoomCommand(CreateRoomDto Dto, string OwnerId) : IRequest<RoomDto>;
}
