using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class Medication
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "El nombre del medicamento es obligatorio")]
        [StringLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        [Display(Name = "Nombre Comercial")]
        public string MedicationName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        [Display(Name = "Nombre Genérico")]
        public string? GenericName { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Fabricante")]
        public string? Manufacturer { get; set; }

        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        [Display(Name = "Concentración")]
        public string? Concentration { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Categoría")]
        public string? Category { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}