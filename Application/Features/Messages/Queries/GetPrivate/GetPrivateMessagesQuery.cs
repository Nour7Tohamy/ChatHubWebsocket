using Application.DTOs.MessageDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Messages.Queries.GetPrivate
{
    public record GetPrivateMessagesQuery(string SenderId, string ReceiverId) : IRequest<IEnumerable<MessageDto>>;

}
