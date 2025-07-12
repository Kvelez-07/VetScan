using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class MedicalRecordListViewModel
    {
        public int MedicalRecordId { get; set; }

        [Display(Name = "Número de Registro")]
        public string RecordNumber { get; set; } = string.Empty;

        [Display(Name = "Mascota")]
        public string PetName { get; set; } = string.Empty;

        public int PetId { get; set; }

        [Display(Name = "Dueño")]
        public string OwnerName { get; set; } = string.Empty;

        [Display(Name = "Fecha de Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        // Propiedades calculadas (no necesitan annotations)
        public string FormattedCreationDate => CreationDate.ToString("dd/MM/yyyy HH:mm");
        public string StatusClass => Status == "Active" ? "status-active" : "status-inactive";
    }
}