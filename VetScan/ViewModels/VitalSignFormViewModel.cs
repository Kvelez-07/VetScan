// ViewModels/VitalSignFormViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class VitalSignFormViewModel
    {
        public int VitalSignId { get; set; }

        [Required]
        [Display(Name = "Consulta")]
        public int ConsultationId { get; set; }

        [Display(Name = "Temperatura (°C)")]
        [Range(30.0, 45.0)]
        public decimal? Temperature { get; set; }

        [Display(Name = "Frecuencia Cardíaca (lpm)")]
        [Range(20, 300)]
        public int? HeartRate { get; set; }

        [Display(Name = "Frecuencia Respiratoria (rpm)")]
        [Range(5, 100)]
        public int? RespiratoryRate { get; set; }

        [Display(Name = "Peso (kg)")]
        [Range(0.1, 200.0)]
        public decimal? Weight { get; set; }

        [Display(Name = "Presión Sistólica")]
        [Range(50, 250)]
        public int? BloodPressureSystolic { get; set; }

        [Display(Name = "Presión Diastólica")]
        [Range(30, 150)]
        public int? BloodPressureDiastolic { get; set; }

        [Display(Name = "Notas Adicionales")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime RecordedDate { get; set; } = DateTime.Now;
    }
}