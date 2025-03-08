using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class MaterialTag
    {
        [Key]
        public string Id { get; set; }

        public string MaterialId { get; set; }

        public string TagId { get; set; }

        // Relacionamentos
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}
