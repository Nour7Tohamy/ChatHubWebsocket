using Application.DTOs.RoomDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rooms.Queries.GetById
{
    public record GetRoomByIdQuery(Guid RoomId) : IRequest<RoomDetailsDto>;

}
