using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class PetFormDto
    {
        [Required(ErrorMessage = "El dueño de la mascota es obligatorio")]
        public int PetOwnerId { get; set; }

        [Required(ErrorMessage = "El código de mascota es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string PetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la mascota es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria")]
        public int SpeciesId { get; set; }

        public int? BreedId { get; set; }

        [StringLength(1, ErrorMessage = "El género debe ser un solo carácter")]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Range(0, 999.99, ErrorMessage = "El peso debe estar entre 0 y 999.99")]
        public decimal? Weight { get; set; }

        [StringLength(100, ErrorMessage = "El color no puede exceder 100 caracteres")]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
