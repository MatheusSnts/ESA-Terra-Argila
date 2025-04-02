using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ESA_Terra_Argila.Enums;


namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Representa um pedido no sistema.
    /// Armazena informações sobre pedidos feitos pelos usuários, incluindo os itens solicitados e o status atual.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Identificador único do pedido.
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Identificador do usuário que realizou o pedido.
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Status atual do pedido (Pendente, Confirmado, Enviado, Entregue, Cancelado).
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Usuário que realizou o pedido. Relacionamento com a entidade User.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Data de criação do pedido, gerada automaticamente.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Coleção de itens incluídos no pedido.
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// Construtor do Order.
        /// Inicializa a data de criação com a data/hora atual e a coleção de itens do pedido.
        /// </summary>
        public Order()
        {
            CreatedAt = DateTime.UtcNow;
            OrderItems = new HashSet<OrderItem>();
        }

        /// <summary>
        /// Calcula o valor total do pedido somando os valores de todos os itens incluídos.
        /// </summary>
        /// <returns>Valor total do pedido em moeda corrente.</returns>
        public float GetTotal()
        {
            return OrderItems.Sum(item => item.GetTotal());
        }
    }
}
