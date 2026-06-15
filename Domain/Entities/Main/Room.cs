using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Main
{
    public class Room : BaseEntity
    {
        public Guid Id { get; set; } 

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;

        public AppUser Owner { get; set; } = null!;
        public ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
