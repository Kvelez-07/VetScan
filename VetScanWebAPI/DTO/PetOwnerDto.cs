using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class PetOwnerDto
    {
        public int PetOwnerId { get; set; }

        [Required]
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string Country { get; set; } = "Costa Rica";

        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(20)]
        public string PreferredContactMethod { get; set; } = "Email";

        public int PetCount { get; set; }
    }
}
