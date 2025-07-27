using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.DTO
{
    public class AppointmentFormDto
    {
        [Required(ErrorMessage = "La mascota es requerida")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        public DateTime AppointmentDate { get; set; }

        [Range(1, 480, ErrorMessage = "La duración debe estar entre 1 y 480 minutos")]
        public int Duration { get; set; } = 30;

        [Required(ErrorMessage = "El tipo de cita es requerido")]
        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        public string AppointmentType { get; set; } = "Consulta General";

        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        public string Status { get; set; } = "Scheduled";

        [StringLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        public string? Notes { get; set; }

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string? ReasonForVisit { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El costo estimado debe estar entre 0 y 999999.99")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El costo real debe estar entre 0 y 999999.99")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ActualCost { get; set; }
    }
}
