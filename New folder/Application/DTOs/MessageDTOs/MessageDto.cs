namespace Application.DTOs.MessageDTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public Guid? RoomId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime SentAt { get; set; }
    }
}