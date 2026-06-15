using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities.Main
{
    public class Message : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// الـ text content — null لو الرسالة voice
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// الـ relative path للـ audio file — null لو الرسالة text
        /// مثال: /voice/userId_1234567890.webm
        /// </summary>
        public string? AudioUrl { get; set; }

        /// <summary>
        /// Text = 0  |  Voice = 1
        /// </summary>
        public MessageType Type { get; set; } = MessageType.Text;

        public string SenderId { get; set; } = default!;
        public AppUser Sender { get; set; } = default!;

        public Guid? RoomId { get; set; }
        public Room? Room { get; set; }

        public string? ReceiverId { get; set; }
        public AppUser? Receiver { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
