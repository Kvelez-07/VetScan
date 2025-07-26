using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class AnimalSpeciesListViewModel
    {
        [Display(Name = "Id Especie")]
        public int SpeciesId { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre de la especie es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string SpeciesName { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        [Display(Name = "Estado")]
        public bool IsActive { get; set; }

        [Display(Name = "Razas")]
        public int BreedCount { get; set; }

        [Display(Name = "Mascotas")]
        public int PetCount { get; set; }

        // Propiedades calculadas
        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activa" : "Inactiva";
    }
}