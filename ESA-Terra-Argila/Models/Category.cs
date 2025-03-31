using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa uma categoria para classificação de materiais e produtos no sistema.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Identificador único da categoria
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do usuário proprietário da categoria
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Nome da categoria
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Referência da categoria
        /// </summary>
        [StringLength(100, ErrorMessage = "A referência não pode ter mais de 100 caracteres")]
        [Display(Name = "Referência")]
        public string Reference { get; set; } = default!;

        /// <summary>
        /// Descrição detalhada da categoria
        /// </summary>
        [StringLength(500, ErrorMessage = "A descrição não pode ter mais de 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        /// <summary>
        /// Indica se a categoria está ativa no sistema
        /// </summary>
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data de criação da categoria
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Usuário proprietário da categoria
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Coleção de itens pertencentes a esta categoria
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Item> Items { get; set; }

        /// <summary>
        /// Coleção de produtos pertencentes a esta categoria
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// Coleção de materiais pertencentes a esta categoria
        /// </summary>
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

        /// <summary>
        /// Inicializa uma nova instância da classe Category
        /// </summary>
        public Category()
        {
            CreatedAt = DateTime.UtcNow;
            Items = new HashSet<Item>();
        }
    }
}
