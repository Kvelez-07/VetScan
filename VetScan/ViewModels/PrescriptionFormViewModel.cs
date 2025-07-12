// ViewModels/PrescriptionFormViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class PrescriptionFormViewModel
    {
        public int PrescriptionId { get; set; }

        [Required]
        [Display(Name = "Consulta")]
        public int ConsultationId { get; set; }

        [Required]
        [Display(Name = "Medicamento")]
        public int MedicationId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Dosis")]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Frecuencia")]
        public string Frequency { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Duración")]
        public string? Duration { get; set; }

        [StringLength(500)]
        [Display(Name = "Instrucciones")]
        public string? Instructions { get; set; }

        [Display(Name = "Cantidad")]
        [Range(1, 1000)]
        public int? Quantity { get; set; }

        [Display(Name = "Reabastecimientos")]
        [Range(0, 10)]
        public int Refills { get; set; } = 0;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Estado")]
        public string Status { get; set; } = "Active";
    }
}