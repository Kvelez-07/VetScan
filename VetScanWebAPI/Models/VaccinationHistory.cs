using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class VaccinationHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VaccinationId { get; set; }

        [Required]
        [Display(Name = "Mascota")]
        public int PetId { get; set; }

        [Required]
        [Display(Name = "Vacuna")]
        public int VaccineId { get; set; }

        [Required]
        [Display(Name = "Veterinario")]
        public int VeterinarianId { get; set; }

        [Required]
        [Display(Name = "Fecha de Vacunación")]
        [DataType(DataType.Date)]
        public DateTime VaccinationDate { get; set; } = DateTime.Today;

        [Display(Name = "Número de Lote")]
        [StringLength(50)]
        public string? BatchNumber { get; set; }

        [Display(Name = "Fecha de Expiración")]
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "Próxima Dosis")]
        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        [Display(Name = "Reacciones")]
        [StringLength(500)]
        public string? Reactions { get; set; }

        // Navigation properties
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; } = null!;

        [ForeignKey("VaccineId")]
        public virtual Vaccine Vaccine { get; set; } = null!;

        [ForeignKey("VeterinarianId")]
        public virtual Veterinarian Veterinarian { get; set; } = null!;
    }
}
