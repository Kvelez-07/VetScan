using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class Specialty
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpecialtyId { get; set; }

        [Required(ErrorMessage = "El nombre de la especialidad es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Especialidad")]
        public string SpecialtyName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
    }
}
