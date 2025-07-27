using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.DTO
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }

        [Required]
        public int PetId { get; set; }
        public string? PetName { get; set; }

        [Required]
        public int VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public int Duration { get; set; }

        [Required]
        public string AppointmentType { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Scheduled";

        public string? Notes { get; set; }
        public string? ReasonForVisit { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ActualCost { get; set; }

    }
}
