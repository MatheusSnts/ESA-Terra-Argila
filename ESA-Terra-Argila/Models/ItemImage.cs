using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class ItemImage
    {
        [Key]
        public int Id { get; set; }

        public int? ItemId { get; set; }

        public string Name { get; set; } = default!;

        public string Path { get; set; } = default!;

        [ForeignKey("ItemId")]
        [JsonIgnore]
        public virtual Item? Item { get; set; }

    }
}
