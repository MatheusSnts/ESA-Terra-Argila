namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa a relação entre um produto e os materiais que o compõem.
    /// Define a quantidade de cada material usado em um produto.
    /// </summary>
    public class ProductMaterial
    {
        /// <summary>
        /// Identificador do produto
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// Identificador do material
        /// </summary>
        public int MaterialId { get; set; }
        
        /// <summary>
        /// Quantidade do material utilizada no produto
        /// </summary>
        public float Quantity { get; set; }

        /// <summary>
        /// Estoque do material utilizado no produto
        /// </summary>
        public float Stock { get; set; }

        /// <summary>
        /// Referência para o produto
        /// </summary>
        public virtual Product Product { get; set; } = default!;
        
        /// <summary>
        /// Referência para o material
        /// </summary>
        public virtual Material Material { get; set; } = default!;
    }
}
