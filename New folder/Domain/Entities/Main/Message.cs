using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Main
{
    public class Message : BaseEntity
    {
        public string Content { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public Guid? RoomId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public AppUser Sender { get; set; } = null!;
        public Room? Room { get; set; }
        public AppUser? Receiver { get; set; }
    }
}
