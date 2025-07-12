namespace VetScan.ViewModels
{
    public class MedicalConsultationListViewModel
    {
        public int ConsultationId { get; set; }
        public string RecordNumber { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
        public DateTime ConsultationDate { get; set; }
        public string ConsultationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FormattedDate => ConsultationDate.ToString("dd/MM/yyyy HH:mm");
        public string StatusClass => Status == "Completed" ? "status-completed" : "status-pending";
    }
}
