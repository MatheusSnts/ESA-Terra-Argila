using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Display(Name = "Categoria")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome do Material")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "O campo Referência é obrigatório.")]
        [StringLength(50, ErrorMessage = "A Referência deve ter no máximo 50 caracteres.")]
        [Display(Name = "Código de Referência")]
        public string Reference { get; set; } = default!;

        [Required(ErrorMessage = "O campo Descrição é obrigatório.")]
        [StringLength(500, ErrorMessage = "A Descrição deve ter no máximo 500 caracteres.")]
        [Display(Name = "Descrição Completa")]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "O campo Preço é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O Preço deve ser maior que 0,00 €.")]
        [Display(Name = "Preço")]
        public float Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O Stock não pode ser negativo")]
        [Display(Name = "Stock")]
        public float Stock { get; set; } = 0;

        [Required(ErrorMessage = "O campo Unidade é obrigatório.")]
        [StringLength(20, ErrorMessage = "A Unidade deve ter no máximo 20 caracteres.")]
        [Display(Name = "Unidade de Medida")]
        public string Unit { get; set; } = default!;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        // Relacionamentos
        [ForeignKey("CategoryId")]
        [Display(Name = "Categoria")]
        public virtual Category? Category { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Display(Name = "Produtos")]
        [JsonIgnore]
        public ICollection<ProductMaterial> ProductMaterials { get; set; }

        [Display(Name = "Tags")]
        [JsonIgnore]
        public virtual ICollection<Tag> Tags { get; set; }

        [Display(Name = "Imagens")]
        public virtual ICollection<MaterialImage> Images { get; set; }

        public ICollection<UserMaterialFavorite> FavoritedByUsers { get; set; }

        public Material()
        {
            CreatedAt = DateTime.UtcNow;
            ProductMaterials = new HashSet<ProductMaterial>();
            Tags = new HashSet<Tag>();
            Images = new HashSet<MaterialImage>();
            FavoritedByUsers = new HashSet<UserMaterialFavorite>();
        }
    }
}
