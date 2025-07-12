// Models/VitalSign.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class VitalSign
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VitalSignId { get; set; }

        [Required]
        [Display(Name = "Consulta")]
        public int ConsultationId { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Display(Name = "Temperatura (°C)")]
        [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30.0 y 45.0")]
        public decimal? Temperature { get; set; }

        [Display(Name = "Frecuencia Cardíaca (lpm)")]
        [Range(20, 300, ErrorMessage = "La frecuencia cardíaca debe estar entre 20 y 300")]
        public int? HeartRate { get; set; }

        [Display(Name = "Frecuencia Respiratoria (rpm)")]
        [Range(5, 100, ErrorMessage = "La frecuencia respiratoria debe estar entre 5 y 100")]
        public int? RespiratoryRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Peso (kg)")]
        [Range(0.1, 200.0, ErrorMessage = "El peso debe estar entre 0.1 y 200.0")]
        public decimal? Weight { get; set; }

        [Display(Name = "Presión Sistólica")]
        [Range(50, 250, ErrorMessage = "La presión sistólica debe estar entre 50 y 250")]
        public int? BloodPressureSystolic { get; set; }

        [Display(Name = "Presión Diastólica")]
        [Range(30, 150, ErrorMessage = "La presión diastólica debe estar entre 30 y 150")]
        public int? BloodPressureDiastolic { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime RecordedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Notas Adicionales")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("ConsultationId")]
        public virtual MedicalConsultation Consultation { get; set; } = null!;
    }
}