using Application.DTOs.MessageDTOs;
using Application.Infrastructure.Repositories;
using Application.Infrastructure.Services.Messages;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Infrastructure.Services.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public MessageService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<MessageDto> SendAsync(string senderId, SendMessageDto dto)
        {
            var user = await _userManager.FindByIdAsync(senderId)
                ?? throw new Exception("Sender not found");

            var message = new Message
            {
                Content = dto.Content,
                SenderId = senderId,
                RoomId = dto.RoomId,
                ReceiverId = dto.ReceiverId,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SenderId = senderId,
                SenderName = user.DisplayName ?? user.UserName!,
                RoomId = message.RoomId,
                ReceiverId = message.ReceiverId,
                SentAt = message.SentAt
            };
        }
    }
}