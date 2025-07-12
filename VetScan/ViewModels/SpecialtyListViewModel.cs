// SpecialtyListViewModel.cs
namespace VetScan.ViewModels
{
    public class SpecialtyListViewModel
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activa" : "Inactiva";
        public int VeterinarianCount { get; set; }
    }
}