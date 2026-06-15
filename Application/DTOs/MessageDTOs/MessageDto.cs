using Domain.Enums;

namespace Application.DTOs.MessageDTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public string? Content { get; set; }

    /// <summary>
    /// موجود بس لو Type == Voice
    /// </summary>
    public string? AudioUrl { get; set; }

    /// <summary>
    /// 0 = Text  |  1 = Voice
    /// الـ JS بيتحقق منه عشان يعرض text أو &lt;audio&gt;
    /// </summary>
    public MessageType Type { get; set; }

    public string SenderId { get; set; } = default!;
    public string SenderName { get; set; } = default!;
    public Guid? RoomId { get; set; }
    public string? ReceiverId { get; set; }
    public DateTime SentAt { get; set; }
}