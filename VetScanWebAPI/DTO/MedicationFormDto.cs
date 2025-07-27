using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class MedicationFormDto
    {
        [Required(ErrorMessage = "El nombre del medicamento es obligatorio")]
        [StringLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        public string MedicationName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        public string? GenericName { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string? Manufacturer { get; set; }

        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        public string? Concentration { get; set; }

        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
