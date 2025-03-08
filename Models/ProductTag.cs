using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class ProductTag
    {
        [Key]
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string TagId { get; set; }

        // Relacionamentos
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}
