using Domain.Entities.Main;

public class Notification
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = default!;
}