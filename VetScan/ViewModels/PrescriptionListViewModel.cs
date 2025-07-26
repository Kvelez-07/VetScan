using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class PrescriptionListViewModel
    {
        [Display(Name = "Id Prescripción")]
        public int PrescriptionId { get; set; }

        [Display(Name = "Id Consulta")]
        public int ConsultationId { get; set; }

        [Display(Name = "Consulta")]
        public string ConsultationInfo { get; set; } = string.Empty;

        [Display(Name = "Mascota")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "Medicamento")]
        public string MedicationName { get; set; } = string.Empty;

        [Display(Name = "Dosis")]
        public string Dosage { get; set; } = string.Empty;

        [Display(Name = "Frecuencia")]
        public string Frequency { get; set; } = string.Empty;

        [Display(Name = "Fecha Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FormattedDate => CreatedDate.ToString("dd/MM/yyyy");
        public string StatusClass => Status == "Active" ? "badge bg-success" : "badge bg-secondary";
    }
}