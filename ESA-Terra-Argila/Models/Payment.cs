using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESA_Terra_Argila.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "EUR";

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
