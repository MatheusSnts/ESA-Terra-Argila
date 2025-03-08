using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        // Relacionamentos
        public virtual ICollection<Product> Products { get; set; }

        public virtual ICollection<Material> Materials { get; set; }

        public Category()
        {
            Products = new HashSet<Product>();
            Materials = new HashSet<Material>();
        }
    }
}
