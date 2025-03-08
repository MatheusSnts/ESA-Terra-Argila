using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        [JsonIgnore]
        public virtual ICollection<ProductTag> ProductTags { get; set; }

        [JsonIgnore]
        public virtual ICollection<MaterialTag> MaterialTags { get; set; }

        public Tag()
        {
            ProductTags = new HashSet<ProductTag>();
            MaterialTags = new HashSet<MaterialTag>();
        }
    }
}
