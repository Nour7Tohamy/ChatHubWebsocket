using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.ViewModel
{
    public class CreateRoomViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string Description { get; set; } = string.Empty;
    }
}
