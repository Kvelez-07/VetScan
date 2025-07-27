using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class MedicationDto
    {
        public int MedicationId { get; set; }

        [Required]
        [StringLength(200)]
        public string MedicationName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? GenericName { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(50)]
        public string? Concentration { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
