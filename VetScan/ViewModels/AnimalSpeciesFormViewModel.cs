using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class AnimalSpeciesFormViewModel
    {
        public int SpeciesId { get; set; }

        [Required(ErrorMessage = "El nombre de la especie es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre de la Especie")]
        public string SpeciesName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;
    }
}