using System.ComponentModel.DataAnnotations;

namespace MathematicalRhythm.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Contrasena { get; set; }
    }
}
