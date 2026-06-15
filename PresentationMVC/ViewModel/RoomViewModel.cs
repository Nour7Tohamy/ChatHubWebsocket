namespace PresentationMVC.ViewModel
{
    public class RoomViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int MemberCount { get; set; }

        public bool IsMember { get; set; }
    }
}
