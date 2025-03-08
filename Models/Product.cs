using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = default!;

        public int CategoryId { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; } = default!;

        [Display(Name = "Ref.")]
        public string Reference { get; set; } = default!;

        [Display(Name = "Descrição")]
        public string Description { get; set; } = default!;

        [Display(Name = "Preço")]
        public float Price { get; set; }

        [Display(Name = "Unidade")]
        public string Unit { get; set; } = default!;

        // Relacionamentos
        [ForeignKey("CategoryId")]
        [Display(Name = "Categoria")]
        public virtual Category Category { get; set; } = default!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;

        [Display(Name = "Materiais")]
        [JsonIgnore]
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }

        [Display(Name = "Tags")]
        [JsonIgnore]
        public virtual ICollection<ProductTag> ProductTags { get; set; }

        public Product()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            ProductTags = new HashSet<ProductTag>();
        }
    }
}
