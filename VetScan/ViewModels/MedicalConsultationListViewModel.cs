using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class MedicalConsultationListViewModel
    {
        [Display(Name = "Id Consulta")]
        public int ConsultationId { get; set; }

        [Display(Name = "N° Expediente")]
        public string RecordNumber { get; set; } = string.Empty;

        [Display(Name = "Mascota")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "Veterinario")]
        public string VeterinarianName { get; set; } = string.Empty;

        [Display(Name = "Fecha Consulta")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime ConsultationDate { get; set; }

        [Display(Name = "Tipo")]
        public string ConsultationType { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FormattedDate => ConsultationDate.ToString("dd/MM/yyyy HH:mm");
        public string StatusClass => Status == "Completed" ? "status-completed" : "status-pending";
    }
}