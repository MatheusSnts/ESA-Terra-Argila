using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace ESA_Terra_Argila.Models
{
    public class User : IdentityUser
    {
        [PersonalData]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public required string FullName { get; set; }

        [PersonalData]
        [Display(Name = "Morada")]
        public string? Street { get; set; }

        [PersonalData]
        [Display(Name = "Número")]
        public string? StreetNumber { get; set; }

        [PersonalData]
        [Display(Name = "Localidade")]
        public string? City { get; set; }

        [PersonalData]
        [Display(Name = "Código Postal")]
        public string? ZipCode { get; set; }

        [PersonalData]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        [PersonalData]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        public bool AcceptedByAdmin { get; set; } = false;

    }
}
