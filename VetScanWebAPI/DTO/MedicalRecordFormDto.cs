using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class MedicalRecordFormDto
    {
        [Required(ErrorMessage = "La mascota es requerida")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "El número de registro es requerido")]
        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        public string RecordNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string? GeneralNotes { get; set; }

        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        public string Status { get; set; } = "Active";
    }
}
