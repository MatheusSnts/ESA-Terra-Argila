using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace ESA_Terra_Argila.Models
{
    public class User : IdentityUser
    {
        [PersonalData]
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public required string FullName { get; set; }
    }
}
