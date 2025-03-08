using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class ProductMaterial
    {
        [Key]
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string MaterialId { get; set; }

        // Relacionamentos
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }
    }
}
