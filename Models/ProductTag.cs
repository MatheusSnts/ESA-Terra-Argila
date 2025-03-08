using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class ProductTag
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int TagId { get; set; }

        // Relacionamentos
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}
