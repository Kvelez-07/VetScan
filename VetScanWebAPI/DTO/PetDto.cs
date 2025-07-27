using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.DTO
{
    public class PetDto
    {
        public int PetId { get; set; }

        [Required]
        public int PetOwnerId { get; set; }
        public string? PetOwnerName { get; set; }

        [Required]
        [StringLength(20)]
        public string PetCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PetName { get; set; } = string.Empty;

        [Required]
        public int SpeciesId { get; set; }
        public string? SpeciesName { get; set; }

        public int? BreedId { get; set; }
        public string? BreedName { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? AgeDisplay { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; }

        [StringLength(100)]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
