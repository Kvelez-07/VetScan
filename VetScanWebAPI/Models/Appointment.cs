using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class Appointment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentId { get; set; }

        [Required]
        public int PetId { get; set; }

        [Required]
        public int VeterinarianId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public int Duration { get; set; }
        public string AppointmentType { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string? Notes { get; set; }
        public string? ReasonForVisit { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ActualCost { get; set; }

        // Navigation properties
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; } = null!;

        [ForeignKey("VeterinarianId")]
        public virtual Veterinarian Veterinarian { get; set; } = null!;
    }
}
