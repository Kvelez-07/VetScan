using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class BreedDto
    {
        public int BreedId { get; set; }

        [Required]
        public int SpeciesId { get; set; }

        public string SpeciesName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BreedName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
