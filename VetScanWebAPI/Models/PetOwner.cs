using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScanWebAPI.Models
{
    public class PetOwner
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PetOwnerId { get; set; }

        [Required]
        public int UserId { get; set; }

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

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
