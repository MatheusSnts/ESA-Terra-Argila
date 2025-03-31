namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um produto no sistema.
    /// Produtos são itens que podem ser compostos por materiais e são destinados à venda.
    /// </summary>
    public class Product : Item
    {
        /// <summary>
        /// Coleção de relacionamentos entre este produto e os materiais que o compõem
        /// </summary>
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe Product com coleções vazias
        /// </summary>
        public Product()
        {
            ProductMaterials = new HashSet<ProductMaterial>();
            Tags = new HashSet<Tag>();
        }
    }
}
