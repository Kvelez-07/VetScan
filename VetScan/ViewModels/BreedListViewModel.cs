// BreedListViewModel.cs
namespace VetScan.ViewModels
{
    public class BreedListViewModel
    {
        public int BreedId { get; set; }
        public string BreedName { get; set; } = string.Empty;
        public string SpeciesName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activa" : "Inactiva";
    }
}