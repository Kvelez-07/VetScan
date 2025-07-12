// ViewModels/VaccineListViewModel.cs
namespace VetScan.ViewModels
{
    public class VaccineListViewModel
    {
        public int VaccineId { get; set; }
        public string VaccineName { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? VaccineType { get; set; }
        public string? SpeciesName { get; set; }
        public bool IsCore { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Propiedades calculadas
        public string FormattedDate => CreatedDate.ToString("dd/MM/yyyy");
        public string CoreBadgeClass => IsCore ? "badge bg-primary" : "badge bg-secondary";
        public string ActiveBadgeClass => IsActive ? "badge bg-success" : "badge bg-danger";
        public string CoreStatus => IsCore ? "Básica" : "Opcional";
    }
}