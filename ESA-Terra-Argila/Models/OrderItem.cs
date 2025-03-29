using ESA_Terra_Argila.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public float Quantity { get; set; }

        public int? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        public int? ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        public float GetTotal()
        {

            return Item.Price * Quantity;
        }


    }
}
