using ESA_Terra_Argila.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static NuGet.Packaging.PackagingConstants;
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

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        [Display(Name = "Tipo")]
        public UserRole Role { get; set; }

        [Display(Name = "Produtos")]
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }

        [Display(Name = "Materiais")]
        [JsonIgnore]
        public ICollection<Material> Materials { get; set; }

        public ICollection<UserMaterialFavorite> FavoriteMaterials { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        public User()
        {
            CreatedAt = DateTime.UtcNow;
            Materials = new HashSet<Material>();
            FavoriteMaterials = new HashSet<UserMaterialFavorite>();
            Orders = new HashSet<Order>();
        }

    }
}
