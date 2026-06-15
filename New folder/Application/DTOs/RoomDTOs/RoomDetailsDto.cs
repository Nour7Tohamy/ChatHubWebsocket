namespace Application.DTOs.RoomDTOs
{
    public class RoomDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<MemberDto> Members { get; set; } = new();
    }

    public class MemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
