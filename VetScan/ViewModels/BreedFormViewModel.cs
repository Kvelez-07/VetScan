// BreedFormViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class BreedFormViewModel
    {
        public int BreedId { get; set; }

        [Required(ErrorMessage = "La especie es requerida")]
        [Display(Name = "Especie")]
        public int SpeciesId { get; set; }

        [Required(ErrorMessage = "El nombre de la raza es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Raza")]
        public string BreedName { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La descripción no puede exceder 300 caracteres")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;
    }
}