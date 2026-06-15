using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Messages.Commands.Delete
{
    public record DeleteMessageCommand(Guid MessageId, string UserId) : IRequest<bool>;
}
