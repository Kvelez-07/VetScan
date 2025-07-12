using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class MedicationListViewModel
    {
        public int MedicationId { get; set; }

        [DisplayName("Nombre Comercial")]
        [Required(ErrorMessage = "El nombre comercial es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre comercial no puede exceder 200 caracteres")]
        public string MedicationName { get; set; } = string.Empty;

        [DisplayName("Nombre Genérico")]
        [StringLength(200, ErrorMessage = "El nombre genérico no puede exceder 200 caracteres")]
        public string? GenericName { get; set; }

        [DisplayName("Fabricante")]
        [StringLength(100, ErrorMessage = "El fabricante no puede exceder 100 caracteres")]
        public string? Manufacturer { get; set; }

        [DisplayName("Concentración")]
        [StringLength(50, ErrorMessage = "La concentración no puede exceder 50 caracteres")]
        public string? Concentration { get; set; }

        [DisplayName("Categoría")]
        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
        public string? Category { get; set; }

        // Propiedades calculadas para la vista
        [DisplayName("Estado")]
        public string StatusBadgeClass { get; set; } = string.Empty;

        [DisplayName("Fecha de Creación")]
        public string FormattedDate { get; set; } = string.Empty;
    }
}