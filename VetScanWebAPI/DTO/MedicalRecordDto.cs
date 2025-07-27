using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class MedicalRecordDto
    {
        public int MedicalRecordId { get; set; }

        [Required]
        public int PetId { get; set; }
        public string? PetName { get; set; }

        [Required]
        [StringLength(20)]
        public string RecordNumber { get; set; } = string.Empty;

        [Required]
        public DateTime CreationDate { get; set; }

        [StringLength(500)]
        public string? GeneralNotes { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public int ConsultationsCount { get; set; }
    }
}
