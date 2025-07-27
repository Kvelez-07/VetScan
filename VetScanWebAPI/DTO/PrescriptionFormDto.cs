using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class PrescriptionFormDto
    {
        [Required(ErrorMessage = "La consulta es requerida")]
        public int ConsultationId { get; set; }

        [Required(ErrorMessage = "El medicamento es requerido")]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "La dosis es requerida")]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "La frecuencia es requerida")]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string Frequency { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string? Duration { get; set; }

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string? Instructions { get; set; }

        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
        public int? Quantity { get; set; }

        [Range(0, 10, ErrorMessage = "Los reabastecimientos deben estar entre 0 y 10")]
        public int Refills { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        public string Status { get; set; } = "Active";
    }
}
