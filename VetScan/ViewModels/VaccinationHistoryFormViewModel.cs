using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class VaccinationHistoryFormViewModel
    {
        public int VaccinationId { get; set; }

        [Required(ErrorMessage = "La mascota es requerida")]
        [Display(Name = "Mascota")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "La vacuna es requerida")]
        [Display(Name = "Vacuna")]
        public int VaccineId { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        [Display(Name = "Veterinario")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "La fecha de vacunación es requerida")]
        [Display(Name = "Fecha de Vacunación")]
        [DataType(DataType.Date)]
        public DateTime VaccinationDate { get; set; } = DateTime.Today;

        [Display(Name = "Número de Lote")]
        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        public string BatchNumber { get; set; } = string.Empty;

        [Display(Name = "Fecha de Expiración")]
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "Próxima Dosis")]
        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        [Display(Name = "Reacciones")]
        [StringLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        public string Reactions { get; set; } = string.Empty;
    }
}