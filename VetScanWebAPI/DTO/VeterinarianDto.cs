using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class VeterinarianDto
    {
        public int VeterinarianId { get; set; }

        [Required]
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public int? SpecialtyId { get; set; }
        public string? SpecialtyName { get; set; }

        public int YearsOfExperience { get; set; } = 0;

        [StringLength(500)]
        public string? Education { get; set; }

        public int ConsultationCount { get; set; }
        public int AppointmentCount { get; set; }
    }
}
