using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class MedicalRecord
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicalRecordId { get; set; }

        [Required]
        public int PetId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Número de Registro")]
        public string RecordNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Creación")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        [Display(Name = "Notas Generales")]
        public string? GeneralNotes { get; set; }

        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Status { get; set; } = "Active";

        // Navigation properties
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; } = null!;

        public virtual ICollection<MedicalConsultation> MedicalConsultations { get; set; } = new List<MedicalConsultation>();
    }
}
