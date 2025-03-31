using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa uma etiqueta ou marcador para classificar materiais e produtos.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Identificador único da tag
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Código de referência da tag
        /// </summary>
        [Required(ErrorMessage = "O campo Referência é obrigatório.")]
        [StringLength(50, ErrorMessage = "A Referência deve ter no máximo 50 caracteres.")]
        [Display(Name = "Código de Referência")]
        public string Reference { get; set; } = default!;

        /// <summary>
        /// Identificador do usuário que criou a tag
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Nome da tag
        /// </summary>
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da Tag")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Indica se a tag é pública ou privada
        /// </summary>
        [Required(ErrorMessage = "O campo Visibilidade é obrigatório.")]
        [Display(Name = "Visibilidade")]
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Data de criação da tag
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Coleção de materiais associados a esta tag
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Material> Materials { get; set; }

        /// <summary>
        /// Coleção de produtos associados a esta tag
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }

        /// <summary>
        /// Referência ao usuário que criou a tag
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe Tag
        /// </summary>
        public Tag()
        {
            CreatedAt = DateTime.UtcNow;
            Materials = new HashSet<Material>();
            Products = new HashSet<Product>();
        }
    }
}
