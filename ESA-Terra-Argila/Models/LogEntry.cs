using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class LogEntry
    {
        [Key]
        public int Id { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string Action { get; set; } = default!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Ip { get; set; } = default!;
    }
}
