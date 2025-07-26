using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class AppointmentListViewModel
    {
        [Display(Name = "Id Cita")]
        public int AppointmentId { get; set; }

        [Display(Name = "Mascota")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "Veterinario")]
        public string VeterinarianName { get; set; } = string.Empty;

        [Display(Name = "Fecha Cita")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Tipo")]
        public string AppointmentType { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FormattedDate => AppointmentDate.ToString("dd/MM/yyyy HH:mm");
        public string StatusClass =>
            Status == "Completed" ? "status-completed" :
            Status == "Cancelled" ? "status-cancelled" : "status-scheduled";
    }
}