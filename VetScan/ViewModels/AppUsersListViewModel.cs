using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class AppUsersListViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string? PhoneNumber { get; set; }

        // Propiedades para mapeo
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}