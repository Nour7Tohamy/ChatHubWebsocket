namespace Application.DTOs.RoomDTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MembersCount { get; set; }
    }
}
