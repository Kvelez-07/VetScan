using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Usuario o Correo Electrónico")]
        [Required(ErrorMessage = "El usuario o correo electrónico es obligatorio")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? RecaptchaToken { get; set; }
    }
}