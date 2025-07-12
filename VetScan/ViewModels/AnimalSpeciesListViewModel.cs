namespace VetScan.ViewModels
{
    public class AnimalSpeciesListViewModel
    {
        public int SpeciesId { get; set; }
        public string SpeciesName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int BreedCount { get; set; }
        public int PetCount { get; set; }

        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activa" : "Inactiva";
    }
}