using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rooms.Commands.Delete
{
    public record DeleteRoomCommand(Guid RoomId, string UserId) : IRequest<bool>;
}
