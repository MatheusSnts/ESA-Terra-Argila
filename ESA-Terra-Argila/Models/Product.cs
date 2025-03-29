
namespace ESA_Terra_Argila.Models
{
    public class Product : Item
    {
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }

        public Product()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            Tags = new HashSet<Tag>();
        }
    }
}
