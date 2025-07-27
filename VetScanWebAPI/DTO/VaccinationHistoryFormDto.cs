using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class VaccinationHistoryFormDto
    {
        [Required(ErrorMessage = "La mascota es requerida")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "La vacuna es requerida")]
        public int VaccineId { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "La fecha de vacunación es requerida")]
        public DateTime VaccinationDate { get; set; } = DateTime.Today;

        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        public string? BatchNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string? Reactions { get; set; }
    }
}
