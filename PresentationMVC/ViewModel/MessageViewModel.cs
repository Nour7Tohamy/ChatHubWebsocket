namespace PresentationMVC.ViewModel
{
    public class MessageViewModel
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string SenderId { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
    }
}
