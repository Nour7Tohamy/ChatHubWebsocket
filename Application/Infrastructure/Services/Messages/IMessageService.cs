using Application.DTOs.MessageDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Infrastructure.Services.Messages
{
    public interface IMessageService
    {
        Task<MessageDto> SendAsync(string senderId, SendMessageDto dto);
    }
}
