using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Main
{
    public class RoomMember
    {
        public Guid RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Room Room { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }
}
