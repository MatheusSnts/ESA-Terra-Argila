using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        // Relacionamentos
        public virtual ICollection<ProductTag> ProductTags { get; set; }

        public virtual ICollection<MaterialTag> MaterialTags { get; set; }

        public Tag()
        {
            ProductTags = new HashSet<ProductTag>();
            MaterialTags = new HashSet<MaterialTag>();
        }
    }
}
