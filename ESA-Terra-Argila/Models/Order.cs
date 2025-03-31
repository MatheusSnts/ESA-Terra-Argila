using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ESA_Terra_Argila.Enums;


namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um pedido no sistema, contendo múltiplos itens e informações sobre o cliente.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Identificador único do pedido
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Identificador do usuário (cliente) que realizou o pedido
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Data em que o pedido foi criado
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Data em que o pedido foi finalizado ou concluído
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Status atual do pedido
        /// </summary>
        public OrderStatus Status { get; set; }
        
        /// <summary>
        /// Observações adicionais sobre o pedido
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
        
        /// <summary>
        /// Referência ao usuário (cliente) que fez o pedido
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        /// <summary>
        /// Coleção de itens incluídos neste pedido
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; }


        /// <summary>
        /// Inicializa uma nova instância da classe Order com valores padrão
        /// </summary>
        public Order()
        {
            CreatedAt = DateTime.UtcNow;
            OrderItems = new HashSet<OrderItem>();
        }

        public float GetTotal()
        {
            return OrderItems.Sum(item => item.GetTotal());
        }
    }
}
