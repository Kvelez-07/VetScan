using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class Breed
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        // Navigation properties
        [ForeignKey("SpeciesId")]
        [Display(Name = "Especie")]
        public virtual AnimalSpecies Species { get; set; } = null!;

        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
