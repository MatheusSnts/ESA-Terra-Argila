using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa uma categoria para produtos e materiais no sistema.
    /// Permite organizar itens em grupos lógicos para facilitar a navegação e filtragem.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Código de referência da categoria. Obrigatório, máximo de 50 caracteres.
        /// </summary>
        [Required(ErrorMessage = "O campo Referência é obrigatório.")]
        [StringLength(50, ErrorMessage = "A Referência deve ter no máximo 50 caracteres.")]
        [Display(Name = "Código de Referência")]
        public string Reference { get; set; } = default!;

        /// <summary>
        /// Identificador do usuário que criou a categoria.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Nome da categoria. Obrigatório, máximo de 100 caracteres.
        /// </summary>
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da Categoria")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Data de criação da categoria, gerada automaticamente.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Coleção de produtos associados a esta categoria.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }
        
        /// <summary>
        /// Coleção de materiais associados a esta categoria.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Material> Materials { get; set; }

        /// <summary>
        /// Usuário que criou a categoria. Relacionamento com a entidade User.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Construtor da Category.
        /// Inicializa a data de criação com a data/hora atual e as coleções de produtos e materiais.
        /// </summary>
        public Category()
        {
            CreatedAt = DateTime.UtcNow;
            Materials = new HashSet<Material>();
            Products = new HashSet<Product>();
        }
    }
}
