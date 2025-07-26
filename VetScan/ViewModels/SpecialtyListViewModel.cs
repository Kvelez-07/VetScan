using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class SpecialtyListViewModel
    {
        [Display(Name = "Id Especialidad")]
        public int SpecialtyId { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre de la especialidad es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string SpecialtyName { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        [Display(Name = "Estado")]
        public bool IsActive { get; set; }

        [Display(Name = "Veterinarios")]
        public int VeterinarianCount { get; set; }

        // Propiedades calculadas
        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activa" : "Inactiva";
    }
}