
namespace ESA_Terra_Argila.Models
{
    public class Material : Item
    {
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public ICollection<UserMaterialFavorite> FavoritedByUsers { get; set; }

        public Material()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            Tags = new HashSet<Tag>();
            FavoritedByUsers = new HashSet<UserMaterialFavorite>();
        }
    }
}
