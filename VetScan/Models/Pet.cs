using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class Pet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PetId { get; set; }

        [Required]
        public int PetOwnerId { get; set; }

        [Required]
        [StringLength(20)]
        public string PetCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PetName { get; set; } = string.Empty;

        [Required]
        public int SpeciesId { get; set; }

        public int? BreedId { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; }

        [StringLength(100)]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("PetOwnerId")]
        public virtual PetOwner PetOwner { get; set; } = null!;

        [ForeignKey("SpeciesId")]
        public virtual AnimalSpecies Species { get; set; } = null!;

        [ForeignKey("BreedId")]
        public virtual Breed? Breed { get; set; }

        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<VaccinationHistory> VaccinationHistories { get; set; } = new List<VaccinationHistory>();

        // En el modelo Pet.cs
        [NotMapped]
        public string AgeDisplay
        {
            get
            {
                if (!DateOfBirth.HasValue)
                    return "N/A";

                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;

                if (DateOfBirth.Value.Date > today.AddYears(-age))
                    age--;

                if (age == 0)
                {
                    var months = today.Month - DateOfBirth.Value.Month;
                    if (DateOfBirth.Value.Date > today.AddMonths(-months))
                        months--;

                    return $"{months} mes{(months != 1 ? "es" : "")}";
                }

                return $"{age} año{(age != 1 ? "s" : "")}";
            }
        }
    }
}