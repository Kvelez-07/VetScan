using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class AnimalSpecies
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpeciesId { get; set; }

        [Required]
        [StringLength(50)]
        public string SpeciesName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Breed> Breeds { get; set; } = new List<Breed>();
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public virtual ICollection<Vaccine> Vaccines { get; set; } = new List<Vaccine>();
    }
}