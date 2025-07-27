using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class VaccinationHistoryDto
    {
        public int VaccinationId { get; set; }

        [Required]
        public int PetId { get; set; }
        public string? PetName { get; set; }

        [Required]
        public int VaccineId { get; set; }
        public string? VaccineName { get; set; }

        [Required]
        public int VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }

        [Required]
        public DateTime VaccinationDate { get; set; }

        [StringLength(50)]
        public string? BatchNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public DateTime? NextDueDate { get; set; }

        [StringLength(500)]
        public string? Reactions { get; set; }
    }
}
