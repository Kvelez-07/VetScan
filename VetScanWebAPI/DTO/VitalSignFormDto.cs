using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.DTO
{
    public class VitalSignFormDto
    {
        [Required(ErrorMessage = "La consulta es requerida")]
        public int ConsultationId { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30.0 y 45.0")]
        public decimal? Temperature { get; set; }

        [Range(20, 300, ErrorMessage = "La frecuencia cardíaca debe estar entre 20 y 300")]
        public int? HeartRate { get; set; }

        [Range(5, 100, ErrorMessage = "La frecuencia respiratoria debe estar entre 5 y 100")]
        public int? RespiratoryRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0.1, 200.0, ErrorMessage = "El peso debe estar entre 0.1 y 200.0")]
        public decimal? Weight { get; set; }

        [Range(50, 250, ErrorMessage = "La presión sistólica debe estar entre 50 y 250")]
        public int? BloodPressureSystolic { get; set; }

        [Range(30, 150, ErrorMessage = "La presión diastólica debe estar entre 30 y 150")]
        public int? BloodPressureDiastolic { get; set; }

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string? Notes { get; set; }
    }
}
