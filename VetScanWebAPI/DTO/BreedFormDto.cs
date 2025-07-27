using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class BreedFormDto
    {
        [Required(ErrorMessage = "La especie es requerida")]
        public int SpeciesId { get; set; }

        [Required(ErrorMessage = "El nombre de la raza es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string BreedName { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La descripción no puede exceder 300 caracteres")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
