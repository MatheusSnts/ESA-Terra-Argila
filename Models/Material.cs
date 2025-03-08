using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string Reference { get; set; }

        public string Description { get; set; }

        public float Price { get; set; }

        public string Unit { get; set; }

        // Relacionamentos
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }

        public virtual ICollection<MaterialTag> MaterialTags { get; set; }

        public Material()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            MaterialTags = new HashSet<MaterialTag>();
        }
    }
}
