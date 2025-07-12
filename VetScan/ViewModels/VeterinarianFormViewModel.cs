using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class VeterinarianFormViewModel
    {
        // Datos básicos del usuario (solo lectura)
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        // Datos específicos de Veterinarian
        [Display(Name = "Años de Experiencia")]
        [Range(0, 50, ErrorMessage = "Debe ser entre 0 y 50 años")]
        public int YearsOfExperience { get; set; } = 0;

        [Display(Name = "Educación/Formación")]
        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string? Education { get; set; }

        [Display(Name = "Especialidad")]
        public int? SpecialtyId { get; set; }
    }
}