using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class RequestPasswordResetViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        public string? RecaptchaToken { get; set; }
    }
}
