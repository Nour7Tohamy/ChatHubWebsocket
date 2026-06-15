using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
