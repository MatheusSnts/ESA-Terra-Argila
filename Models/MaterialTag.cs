using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class MaterialTag
    {
        [Key]
        public int Id { get; set; }

        public int MaterialId { get; set; }

        public int TagId { get; set; }

        // Relacionamentos
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}
