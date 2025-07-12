// AppointmentListViewModel.cs
using System;

namespace VetScan.ViewModels
{
    public class AppointmentListViewModel
    {
        public int AppointmentId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string FormattedDate => AppointmentDate.ToString("dd/MM/yyyy HH:mm");
        public string AppointmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass =>
            Status == "Completed" ? "status-completed" :
            Status == "Cancelled" ? "status-cancelled" : "status-scheduled";
    }
}