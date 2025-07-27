using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class VeterinarianFormDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public int UserId { get; set; }

        public int? SpecialtyId { get; set; }

        [Range(0, 100, ErrorMessage = "Los años de experiencia deben estar entre 0 y 100")]
        public int YearsOfExperience { get; set; } = 0;

        [StringLength(500, ErrorMessage = "La educación no puede exceder 500 caracteres")]
        public string? Education { get; set; }
    }
}
