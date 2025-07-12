// Models/Prescription.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class Prescription
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PrescriptionId { get; set; }

        [Required]
        [Display(Name = "Consulta")]
        public int ConsultationId { get; set; }

        [Required]
        [Display(Name = "Medicamento")]
        public int MedicationId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Dosis")]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Frecuencia")]
        public string Frequency { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Duración")]
        public string? Duration { get; set; }

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        [Display(Name = "Instrucciones")]
        public string? Instructions { get; set; }

        [Display(Name = "Cantidad")]
        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
        public int? Quantity { get; set; }

        [Display(Name = "Reabastecimientos")]
        [Range(0, 10, ErrorMessage = "Los reabastecimientos deben estar entre 0 y 10")]
        public int Refills { get; set; } = 0;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime? EndDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ConsultationId")]
        public virtual MedicalConsultation Consultation { get; set; } = null!;

        [ForeignKey("MedicationId")]
        public virtual Medication Medication { get; set; } = null!;
    }
}