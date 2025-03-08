using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }
        [JsonIgnore]
        public virtual ICollection<Material> Materials { get; set; }

        public Category()
        {
            Products = new HashSet<Product>();
            Materials = new HashSet<Material>();
        }
    }
}
