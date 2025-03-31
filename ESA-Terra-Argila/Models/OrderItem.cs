using ESA_Terra_Argila.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um item específico em um pedido, contendo informações sobre quantidade e preço.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Identificador único do item do pedido
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do pedido ao qual este item pertence
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Identificador do item (produto ou material) que foi pedido
        /// </summary>
        public int? ItemId { get; set; }

        /// <summary>
        /// Quantidade do item no pedido
        /// </summary>
        [Range(0.01, float.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public float Quantity { get; set; }

        /// <summary>
        /// Preço unitário do item no momento do pedido
        /// </summary>
        [Range(0.01, float.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Preço total do item no pedido (quantidade x preço unitário)
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Referência para o pedido ao qual este item pertence
        /// </summary>
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Referência para o item (produto ou material) pedido
        /// </summary>
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        public float GetTotal()
        {
            return Item.Price * Quantity;
        }
    }
}
