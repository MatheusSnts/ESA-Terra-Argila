using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESA_Terra_Argila.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = default!;

        [Required]
        public float Quantity { get; set; }

        [Required]
        public string Type { get; set; } = default!; // Entrada ou Saída

        [Required]
        public DateTime Date { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}