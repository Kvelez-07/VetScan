using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class SpecialtyDto
    {
        public int SpecialtyId { get; set; }

        [Required]
        [StringLength(100)]
        public string SpecialtyName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
