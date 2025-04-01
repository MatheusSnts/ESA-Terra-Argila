using ESA_Terra_Argila.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static NuGet.Packaging.PackagingConstants;
namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um usuário no sistema.
    /// Estende a classe IdentityUser do ASP.NET Identity para incluir informações adicionais específicas do sistema.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Nome completo do usuário. Campo obrigatório.
        /// </summary>
        [PersonalData]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public required string FullName { get; set; }

        /// <summary>
        /// Endereço (rua) do usuário.
        /// </summary>
        [PersonalData]
        [Display(Name = "Morada")]
        public string? Street { get; set; }

        /// <summary>
        /// Número da residência/estabelecimento do usuário.
        /// </summary>
        [PersonalData]
        [Display(Name = "Número")]
        public string? StreetNumber { get; set; }

        /// <summary>
        /// Cidade/localidade do usuário.
        /// </summary>
        [PersonalData]
        [Display(Name = "Localidade")]
        public string? City { get; set; }

        /// <summary>
        /// Código postal do endereço do usuário.
        /// </summary>
        [PersonalData]
        [Display(Name = "Código Postal")]
        public string? ZipCode { get; set; }

        /// <summary>
        /// Website pessoal ou da empresa do usuário.
        /// </summary>
        [PersonalData]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        /// <summary>
        /// Descrição ou biografia do usuário.
        /// </summary>
        [PersonalData]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        /// <summary>
        /// Indica se o usuário foi aceito/aprovado por um administrador.
        /// Valor padrão é false (não aprovado).
        /// </summary>
        public bool AcceptedByAdmin { get; set; } = false;

        /// <summary>
        /// Data de criação da conta do usuário, gerada automaticamente.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Papel/função do usuário no sistema (Artesão, Cliente, Admin).
        /// Este campo não é persistido diretamente no banco de dados.
        /// </summary>
        [NotMapped]
        [Display(Name = "Tipo")]
        public UserRole Role { get; set; }

        /// <summary>
        /// Coleção de produtos associados a este usuário (criados ou gerenciados por ele).
        /// </summary>
        [Display(Name = "Produtos")]
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }

        /// <summary>
        /// Coleção de materiais associados a este usuário (criados ou gerenciados por ele).
        /// </summary>
        [Display(Name = "Materiais")]
        [JsonIgnore]
        public ICollection<Material> Materials { get; set; }

        /// <summary>
        /// Coleção de materiais marcados como favoritos por este usuário.
        /// </summary>
        public ICollection<UserMaterialFavorite> FavoriteMaterials { get; set; }
        
        /// <summary>
        /// Coleção de pedidos feitos por este usuário.
        /// </summary>
        public virtual ICollection<Order> Orders { get; set; }

        /// <summary>
        /// Construtor do User.
        /// Inicializa a data de criação com a data/hora atual e todas as coleções associadas.
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
