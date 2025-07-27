using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class MedicalConsultationDto
    {
        public int ConsultationId { get; set; }

        [Required]
        public int MedicalRecordId { get; set; }
        public string? RecordNumber { get; set; }

        [Required]
        public int VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }

        [Required]
        public DateTime ConsultationDate { get; set; }

        [Required]
        public string ConsultationType { get; set; } = "General";

        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }

        public DateTime? NextAppointmentRecommended { get; set; }

        [Required]
        public string Status { get; set; } = "Completed";

        public int VitalSignsCount { get; set; }
        public int PrescriptionsCount { get; set; }
    }
}
