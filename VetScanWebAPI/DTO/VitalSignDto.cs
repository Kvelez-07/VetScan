using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.DTO
{
    public class VitalSignDto
    {
        public int VitalSignId { get; set; }

        [Required]
        public int ConsultationId { get; set; }
        public string? ConsultationType { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Range(30.0, 45.0)]
        public decimal? Temperature { get; set; }

        [Range(20, 300)]
        public int? HeartRate { get; set; }

        [Range(5, 100)]
        public int? RespiratoryRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0.1, 200.0)]
        public decimal? Weight { get; set; }

        [Range(50, 250)]
        public int? BloodPressureSystolic { get; set; }

        [Range(30, 150)]
        public int? BloodPressureDiastolic { get; set; }

        public DateTime RecordedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
