using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class PetOwnerFormViewModel
    {
        // Datos básicos del usuario (solo lectura)
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        // Datos específicos de PetOwner
        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "El estado/provincia no puede exceder 100 caracteres")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
        [Display(Name = "Código Postal")]
        public string? PostalCode { get; set; }

        [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
        public string Country { get; set; } = "Costa Rica";

        [StringLength(200, ErrorMessage = "El nombre de contacto no puede exceder 200 caracteres")]
        [Display(Name = "Nombre Contacto de Emergencia")]
        public string? EmergencyContactName { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Display(Name = "Teléfono de Emergencia")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(20, ErrorMessage = "El método de contacto no puede exceder 20 caracteres")]
        [Display(Name = "Método de Contacto Preferido")]
        public string PreferredContactMethod { get; set; } = "Email";
    }
}