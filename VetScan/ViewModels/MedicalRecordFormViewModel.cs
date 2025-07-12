// ViewModels/MedicalRecordFormViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class MedicalRecordFormViewModel
    {
        public int MedicalRecordId { get; set; }

        [Required(ErrorMessage = "La mascota es requerida")]
        [Display(Name = "Mascota")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "El número de registro es requerido")]
        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        [Display(Name = "Número de Registro")]
        public string RecordNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        [Display(Name = "Notas Generales")]
        public string? GeneralNotes { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [Display(Name = "Estado")]
        public string Status { get; set; } = "Active";
    }
}