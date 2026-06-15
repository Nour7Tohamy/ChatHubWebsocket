using Application.DTOs.RoomDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rooms.Queries.GetAll
{
    public record GetAllRoomsQuery : IRequest<IEnumerable<RoomDto>>;
}
