// ViewModels/PrescriptionListViewModel.cs
using System;

namespace VetScan.ViewModels
{
    public class PrescriptionListViewModel
    {
        public int PrescriptionId { get; set; }
        public int ConsultationId { get; set; }
        public string ConsultationInfo { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FormattedDate => CreatedDate.ToString("dd/MM/yyyy");
        public string StatusClass => Status == "Active" ? "badge bg-success" : "badge bg-secondary";
    }
}