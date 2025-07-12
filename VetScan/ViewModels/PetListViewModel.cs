// PetListViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class PetListViewModel
    {
        public int PetId { get; set; }

        [Display(Name = "Código")]
        public string PetCode { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "Especie")]
        public string Species { get; set; } = string.Empty;

        [Display(Name = "Raza")]
        public string? Breed { get; set; }

        [Display(Name = "Dueño")]
        public string OwnerName { get; set; } = string.Empty;

        [Display(Name = "Fecha Nac.")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Edad")]
        public string AgeDisplay
        {
            get
            {
                if (!DateOfBirth.HasValue)
                    return "N/A";

                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;

                if (DateOfBirth.Value.Date > today.AddYears(-age))
                    age--;

                if (age == 0)
                {
                    var months = today.Month - DateOfBirth.Value.Month;
                    if (DateOfBirth.Value.Date > today.AddMonths(-months))
                        months--;

                    return $"{months} mes{(months != 1 ? "es" : "")}";
                }

                return $"{age} año{(age != 1 ? "s" : "")}";
            }
        }

        [Display(Name = "Género")]
        public string? GenderDisplay { get; set; }

        [Display(Name = "Peso (kg)")]
        public decimal? Weight { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }
    }
}