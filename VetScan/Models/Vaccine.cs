// Models/Vaccine.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class Vaccine
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VaccineId { get; set; }

        [Required(ErrorMessage = "El nombre de la vacuna es obligatorio")]
        [StringLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        [Display(Name = "Nombre de la Vacuna")]
        public string VaccineName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Fabricante")]
        public string? Manufacturer { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Tipo de Vacuna")]
        public string? VaccineType { get; set; }

        [Display(Name = "Especie")]
        public int? SpeciesId { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Edad Recomendada")]
        public string? RecommendedAge { get; set; }

        [Display(Name = "Intervalo de Refuerzo (días)")]
        [Range(0, 3650, ErrorMessage = "El intervalo debe estar entre 0 y 3650 días")]
        public int? BoosterInterval { get; set; }

        [Display(Name = "Vacuna Básica")]
        public bool IsCore { get; set; } = false;

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SpeciesId")]
        public virtual AnimalSpecies? Species { get; set; }

        public virtual ICollection<VaccinationHistory> VaccinationHistories { get; set; } = new List<VaccinationHistory>();
    }
}