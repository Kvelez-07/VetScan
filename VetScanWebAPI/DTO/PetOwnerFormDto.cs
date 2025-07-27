using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class PetOwnerFormDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public int UserId { get; set; }

        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "El estado no puede exceder 100 caracteres")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
        public string? PostalCode { get; set; }

        [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
        public string Country { get; set; } = "Costa Rica";

        [StringLength(200, ErrorMessage = "El nombre de contacto de emergencia no puede exceder 200 caracteres")]
        public string? EmergencyContactName { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono de emergencia no puede exceder 20 caracteres")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(20, ErrorMessage = "El método de contacto preferido no puede exceder 20 caracteres")]
        public string PreferredContactMethod { get; set; } = "Email";
    }
}
