using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class AnimalSpeciesFormDto
    {
        [Required(ErrorMessage = "El nombre de la especie es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string SpeciesName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
