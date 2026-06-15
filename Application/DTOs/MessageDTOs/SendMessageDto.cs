using Domain.Enums;

namespace Application.DTOs.MessageDTOs
{

    public class SendMessageDto
    {
        public string? Content { get; set; }   // text content أو null لو voice
        public Guid? RoomId { get; set; }
        public string? ReceiverId { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;

        /// <summary>
        /// الـ relative path للـ audio file بعد رفعه على السيرفر.
        /// بيتبعت بس لو Type == Voice.
        /// </summary>
        public string? AudioUrl { get; set; }
    }
}
