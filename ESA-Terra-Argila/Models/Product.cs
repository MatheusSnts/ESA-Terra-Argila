namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um produto no sistema.
    /// Herda da classe abstrata Item, estendendo-a com propriedades específicas de produtos.
    /// </summary>
    public class Product : Item
    {
        /// <summary>
        /// Coleção de materiais associados ao produto.
        /// Relacionamento com a entidade ProductMaterial que conecta produtos e materiais.
        /// </summary>
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        
        /// <summary>
        /// Coleção de tags associadas ao produto.
        /// Permite categorizar e filtrar produtos por características específicas.
        /// </summary>
        public virtual ICollection<Tag> Tags { get; set; }

        /// <summary>
        /// Construtor do Product.
        /// Inicializa as coleções de materiais e tags associadas ao produto.
        /// </summary>
        public Product()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            Tags = new HashSet<Tag>();
        }
    }
}
