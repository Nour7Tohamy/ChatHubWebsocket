namespace Application.DTOs.MessageDTOs
{
    public class SendMessageDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid? RoomId { get; set; }
        public string? ReceiverId { get; set; }
    }
}
