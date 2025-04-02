using ESA_Terra_Argila.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um item específico dentro de um pedido.
    /// Armazena informações sobre a quantidade e o item solicitado.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Identificador único do item do pedido.
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Quantidade solicitada do item.
        /// </summary>
        public float Quantity { get; set; }

        /// <summary>
        /// Identificador do pedido ao qual este item pertence.
        /// </summary>
        public int? OrderId { get; set; }
        
        /// <summary>
        /// Pedido ao qual este item pertence. Relacionamento com a entidade Order.
        /// </summary>
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Identificador do item (produto ou material) que foi pedido.
        /// </summary>
        public int? ItemId { get; set; }
        
        /// <summary>
        /// Item (produto ou material) que foi pedido. Relacionamento com a entidade Item.
        /// </summary>
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        /// <summary>
        /// Calcula o valor total deste item no pedido (preço unitário × quantidade).
        /// </summary>
        /// <returns>Valor total deste item no pedido em moeda corrente.</returns>
        public float GetTotal()
        {
            return Item.Price * Quantity;
        }
    }
}
