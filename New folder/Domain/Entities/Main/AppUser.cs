using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Main
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    }
}
