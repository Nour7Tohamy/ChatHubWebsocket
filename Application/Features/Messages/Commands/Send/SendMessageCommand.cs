using Application.DTOs.MessageDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Messages.Commands.Send
{
    public record SendMessageCommand(SendMessageDto Dto, string SenderId) : IRequest<MessageDto>;

}
