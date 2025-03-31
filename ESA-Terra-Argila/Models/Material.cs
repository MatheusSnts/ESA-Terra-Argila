namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um material específico no sistema.
    /// Materiais são itens que podem ser utilizados em produtos e incluem
    /// propriedades específicas como tags e relacionamentos com produtos.
    /// </summary>
    public class Material : Item
    {
        /// <summary>
        /// Coleção de relacionamentos entre este material e os produtos que o utilizam
        /// </summary>
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        
        /// <summary>
        /// Coleção de tags associadas a este material para categorização
        /// </summary>
        public virtual ICollection<Tag> Tags { get; set; }
        
        /// <summary>
        /// Coleção de usuários que favoritaram este material
        /// </summary>
        public ICollection<UserMaterialFavorite> FavoritedByUsers { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe Material com coleções vazias
        /// </summary>
        public Material()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            Tags = new HashSet<Tag>();
            FavoritedByUsers = new HashSet<UserMaterialFavorite>();
        }
    }
}
