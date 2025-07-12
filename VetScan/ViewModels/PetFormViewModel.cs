// PetSignUpViewModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.ViewModels
{
    public class PetFormViewModel
    {
        public int PetId { get; set; }  // ID de la mascota

        [Required(ErrorMessage = "El dueño es obligatorio")]
        [Display(Name = "Dueño de la mascota")]
        public int PetOwnerId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la mascota")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria")]
        [Display(Name = "Especie")]
        public int SpeciesId { get; set; }

        [Display(Name = "Raza (opcional)")]
        public int? BreedId { get; set; }

        [Display(Name = "Género")]
        [StringLength(1)]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento (opcional)")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Peso (kg, opcional)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "Peso inválido")]
        public decimal? Weight { get; set; }

        [Display(Name = "Color (opcional)")]
        [StringLength(100)]
        public string? Color { get; set; }

        [Display(Name = "Código de mascota (opcional)")]
        [StringLength(20)]
        public string? PetCode { get; set; }
    }
}