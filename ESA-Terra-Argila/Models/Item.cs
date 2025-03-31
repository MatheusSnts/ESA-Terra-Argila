using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Classe base abstrata para todos os itens do sistema.
    /// Contém as propriedades comuns para materiais e outros tipos de itens.
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// Identificador único do item
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do usuário proprietário do item
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Identificador da categoria do item
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Nome do item
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Código de referência do item
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = default!;

        /// <summary>
        /// Descrição detalhada do item
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = default!;

        /// <summary>
        /// Preço unitário do item
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }

        /// <summary>
        /// Quantidade em estoque do item
        /// </summary>
        [Range(0, double.MaxValue)]
        public float Stock { get; set; } = 0;

        /// <summary>
        /// Unidade de medida do item (ex: kg, m², un)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = default!;

        /// <summary>
        /// Data de criação do registro do item
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Categoria a que o item pertence
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Usuário proprietário do item
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Coleção de imagens associadas ao item
        /// </summary>
        public virtual ICollection<ItemImage> Images { get; set; }

        /// <summary>
        /// Construtor protegido que inicializa as propriedades padrão
        /// </summary>
        protected Item()
        {
            CreatedAt = DateTime.UtcNow;
            Images = new HashSet<ItemImage>();
        }
    }
}
