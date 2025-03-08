using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class ProductMaterial
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int MaterialId { get; set; }

        // Relacionamentos
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }
    }
}
