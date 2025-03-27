using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ESA_Terra_Argila.Enums;


namespace ESA_Terra_Argila.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public OrderStatus Status { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }


        public Order()
        {
            CreatedAt = DateTime.UtcNow;
            OrderItems = new HashSet<OrderItem>();
        }
    }
}
