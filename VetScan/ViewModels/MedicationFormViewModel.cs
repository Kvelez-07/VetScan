using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class MedicationFormViewModel
    {
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
    }
}
