using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class SpecialtyFormDto
    {
        [Required(ErrorMessage = "El nombre de la especialidad es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string SpecialtyName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
