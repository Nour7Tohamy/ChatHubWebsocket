using Application.DTOs.MessageDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Messages.Queries.Get
{
    public record GetRoomMessagesQuery(Guid RoomId, int Page, int PageSize) : IRequest<IEnumerable<MessageDto>>;
}
