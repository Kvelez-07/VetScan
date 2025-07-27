using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class MedicalConsultationFormDto
    {
        [Required(ErrorMessage = "El registro médico es requerido")]
        public int MedicalRecordId { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "La fecha de consulta es requerida")]
        public DateTime ConsultationDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de consulta es requerido")]
        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        public string ConsultationType { get; set; } = "General";

        [StringLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        public string? Diagnosis { get; set; }

        [StringLength(2000, ErrorMessage = "No puede exceder 2000 caracteres")]
        public string? Treatment { get; set; }

        public DateTime? NextAppointmentRecommended { get; set; }

        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        public string Status { get; set; } = "Completed";
    }
}
