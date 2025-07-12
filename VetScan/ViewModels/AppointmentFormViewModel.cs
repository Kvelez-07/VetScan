// AppointmentFormViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class AppointmentFormViewModel
    {
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "La mascota es requerida")]
        [Display(Name = "Mascota")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        [Display(Name = "Veterinario")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "La fecha y hora son requeridas")]
        [Display(Name = "Fecha y Hora")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now;

        [Display(Name = "Duración (minutos)")]
        [Range(5, 240, ErrorMessage = "La duración debe ser entre 5 y 240 minutos")]
        public int Duration { get; set; } = 30;

        [Required(ErrorMessage = "El tipo de cita es requerido")]
        [Display(Name = "Tipo de Cita")]
        public string AppointmentType { get; set; } = "Consulta General";

        [Display(Name = "Estado")]
        public string Status { get; set; } = "Scheduled";

        [Display(Name = "Notas")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Razón de la Visita")]
        [StringLength(500)]
        public string? ReasonForVisit { get; set; }

        [Display(Name = "Costo Estimado")]
        [Range(0, 999999.99)]
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Costo Real")]
        [Range(0, 999999.99)]
        public decimal? ActualCost { get; set; }
    }
}