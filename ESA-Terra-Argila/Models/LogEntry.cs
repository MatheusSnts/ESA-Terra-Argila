using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Models
{
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string Ip { get; set; }
    }
}
