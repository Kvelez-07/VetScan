using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class VaccineFormDto
    {
        [Required(ErrorMessage = "El nombre de la vacuna es obligatorio")]
        [StringLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        public string VaccineName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string? Manufacturer { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string? VaccineType { get; set; }

        public int? SpeciesId { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string? RecommendedAge { get; set; }

        [Range(0, 3650, ErrorMessage = "El intervalo debe estar entre 0 y 3650 días")]
        public int? BoosterInterval { get; set; }

        public bool IsCore { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
