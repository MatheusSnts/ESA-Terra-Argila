using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class MaterialImage
    {
        [Key]
        public int Id { get; set; }


        public int? MaterialId { get; set; }

        public string Name { get; set; } = default!;

        public string Path { get; set; } = default!;

        [ForeignKey("MaterialId")]
        [JsonIgnore]
        public virtual Material? Material { get; set; }

    }
}
