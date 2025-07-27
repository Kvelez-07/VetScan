using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }

        [Required]
        public int ConsultationId { get; set; }
        public string? ConsultationType { get; set; }

        [Required]
        public int MedicationId { get; set; }
        public string? MedicationName { get; set; }

        [Required]
        [StringLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Frequency { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Duration { get; set; }

        [StringLength(500)]
        public string? Instructions { get; set; }

        [Range(1, 1000)]
        public int? Quantity { get; set; }

        [Range(0, 10)]
        public int Refills { get; set; } = 0;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; }
    }
}
