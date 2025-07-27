using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class VaccineDto
    {
        public int VaccineId { get; set; }

        [Required]
        [StringLength(200)]
        public string VaccineName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? VaccineType { get; set; }

        public int? SpeciesId { get; set; }
        public string? SpeciesName { get; set; }

        [StringLength(100)]
        public string? RecommendedAge { get; set; }

        [Range(0, 3650)]
        public int? BoosterInterval { get; set; }

        public bool IsCore { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
