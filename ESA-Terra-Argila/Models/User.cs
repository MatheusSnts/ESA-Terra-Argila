using ESA_Terra_Argila.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static NuGet.Packaging.PackagingConstants;
namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um usuário do sistema, estendendo IdentityUser para adicionar
    /// informações específicas do contexto da aplicação.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        [PersonalData]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public required string FullName { get; set; }

        /// <summary>
        /// Endereço do usuário (rua)
        /// </summary>
        [PersonalData]
        [Display(Name = "Morada")]
        public string? Street { get; set; }

        /// <summary>
        /// Número do endereço
        /// </summary>
        [PersonalData]
        [Display(Name = "Número")]
        public string? StreetNumber { get; set; }

        /// <summary>
        /// Cidade do usuário
        /// </summary>
        [PersonalData]
        [Display(Name = "Localidade")]
        public string? City { get; set; }

        /// <summary>
        /// Código postal do usuário
        /// </summary>
        [PersonalData]
        [Display(Name = "Código Postal")]
        public string? ZipCode { get; set; }

        /// <summary>
        /// Site pessoal ou da empresa do usuário
        /// </summary>
        [PersonalData]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        /// <summary>
        /// Descrição ou biografia do usuário
        /// </summary>
        [PersonalData]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        /// <summary>
        /// Indica se o usuário foi aprovado pelo administrador
        /// </summary>
        public bool AcceptedByAdmin { get; set; } = false;

        /// <summary>
        /// Data de criação da conta do usuário
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Papel do usuário no sistema (Fornecedor, Cliente, etc.)
        /// </summary>
        [NotMapped]
        [Display(Name = "Tipo")]
        public UserRole Role { get; set; }

        /// <summary>
        /// Coleção de produtos associados ao usuário
        /// </summary>
        [Display(Name = "Produtos")]
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }

        /// <summary>
        /// Coleção de materiais associados ao usuário
        /// </summary>
        [Display(Name = "Materiais")]
        [JsonIgnore]
        public ICollection<Material> Materials { get; set; }

        /// <summary>
        /// Coleção de materiais favoritados pelo usuário
        /// </summary>
        public ICollection<UserMaterialFavorite> FavoriteMaterials { get; set; }
        
        /// <summary>
        /// Coleção de pedidos feitos pelo usuário
        /// </summary>
        public virtual ICollection<Order> Orders { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe User com valores padrão
        /// </summary>
        public User()
        {
            CreatedAt = DateTime.UtcNow;
            Materials = new HashSet<Material>();
            FavoriteMaterials = new HashSet<UserMaterialFavorite>();
            Orders = new HashSet<Order>();
        }
    }
}
