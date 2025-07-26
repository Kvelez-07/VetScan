using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class VaccineListViewModel
    {
        [Display(Name = "Id Vacuna")]
        public int VaccineId { get; set; }

        [Display(Name = "Nombre de Vacuna")]
        [Required(ErrorMessage = "El nombre de la vacuna es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string VaccineName { get; set; } = string.Empty;

        [Display(Name = "Fabricante")]
        [StringLength(100, ErrorMessage = "El fabricante no puede exceder los 100 caracteres")]
        public string? Manufacturer { get; set; }

        [Display(Name = "Tipo de Vacuna")]
        [StringLength(50, ErrorMessage = "El tipo no puede exceder los 50 caracteres")]
        public string? VaccineType { get; set; }

        [Display(Name = "Especie")]
        public string? SpeciesName { get; set; }

        [Display(Name = "Tipo")]
        public bool IsCore { get; set; }

        [Display(Name = "Estado")]
        public bool IsActive { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        // Propiedades calculadas (sin annotations ya que no se usan en formularios)
        public string FormattedDate => CreatedDate.ToString("dd/MM/yyyy");
        public string CoreBadgeClass => IsCore ? "badge bg-primary" : "badge bg-secondary";
        public string ActiveBadgeClass => IsActive ? "badge bg-success" : "badge bg-danger";
        public string CoreStatus => IsCore ? "Básica" : "Opcional";
    }
}