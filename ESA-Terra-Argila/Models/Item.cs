using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Classe abstrata que representa um item genérico no sistema.
    /// Serve como classe base para diferentes tipos de itens como Produtos e Materiais.
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// Identificador único do item.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do usuário proprietário/criador do item.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Identificador da categoria à qual o item pertence.
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Nome do item. Obrigatório, máximo de 100 caracteres.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Referência/código do item. Obrigatório, máximo de 50 caracteres.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = default!;

        /// <summary>
        /// Descrição detalhada do item. Obrigatório, máximo de 500 caracteres.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = default!;

        /// <summary>
        /// Preço unitário do item. Obrigatório, deve ser maior que zero.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }

        /// <summary>
        /// Quantidade em estoque do item. Valor padrão é zero.
        /// </summary>
        [Range(0, double.MaxValue)]
        public float Stock { get; set; } = 0;

        /// <summary>
        /// Unidade de medida do item (ex: kg, unidade, m²). Obrigatório, máximo de 20 caracteres.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = default!;

        /// <summary>
        /// Data de criação do item, gerada automaticamente.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Categoria à qual o item pertence. Relacionamento com a entidade Category.
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Usuário proprietário/criador do item. Relacionamento com a entidade User.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Coleção de imagens associadas ao item.
        /// </summary>
        public virtual ICollection<ItemImage> Images { get; set; }

        /// <summary>
        /// Construtor do Item. Inicializa a data de criação com a data/hora atual
        /// e inicializa a coleção de imagens.
        /// </summary>
        protected Item()
        {
            CreatedAt = DateTime.UtcNow;
            Images = new HashSet<ItemImage>();
        }
    }
}
