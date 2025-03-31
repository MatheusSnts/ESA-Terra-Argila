using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ESA_Terra_Argila.Models
{
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [ForeignKey("UserId")]
        public User User { get; set; } = default!;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = default!;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(255)]
        public string? UserAgent { get; set; }

        public bool IsSuccess { get; set; }

        [StringLength(500)]
        public string? AdditionalInfo { get; set; }

        public int? EntityId { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = default!;
    }
} 