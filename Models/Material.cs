using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = default!;

        public int CategoryId { get; set; }

        public string Name { get; set; } = default!;

        public string Reference { get; set; } = default!;

        public string Description { get; set; } = default!;

        public float Price { get; set; }

        public string Unit { get; set; } = default!;

        // Relacionamentos
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = default!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;

        [JsonIgnore]
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        [JsonIgnore]
        public virtual ICollection<MaterialTag> MaterialTags { get; set; }

        public Material()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            MaterialTags = new HashSet<MaterialTag>();
        }
    }
}
