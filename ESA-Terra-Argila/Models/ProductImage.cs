using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        public int? ProductId { get; set; }

        public string Name { get; set; } = default!;

        public string Path { get; set; } = default!;

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public virtual Product? Product { get; set; }

    }
}
