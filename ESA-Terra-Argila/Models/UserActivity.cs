using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ESA_Terra_Argila.Models
{
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(255)]
        public string? UserAgent { get; set; }

        public bool IsSuccess { get; set; }

        [StringLength(500)]
        public string? AdditionalInfo { get; set; }

    }
} 