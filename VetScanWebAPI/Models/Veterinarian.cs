using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class Veterinarian
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VeterinarianId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? SpecialtyId { get; set; }
        public int YearsOfExperience { get; set; } = 0;

        [StringLength(500)]
        public string? Education { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("SpecialtyId")]
        public virtual Specialty? Specialty { get; set; }

        public virtual ICollection<MedicalConsultation> MedicalConsultations { get; set; } = new List<MedicalConsultation>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<VaccinationHistory> VaccinationHistories { get; set; } = new List<VaccinationHistory>();
    }
}
