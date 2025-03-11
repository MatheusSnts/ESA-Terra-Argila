using System;

namespace ESA_Terra_Argila.Models
{
    public class LogEntry
    {
        public int Id { get; set; } // Primary Key
        public string UserEmail { get; set; }
        public string Action { get; set; } // "Login" or "Logout"
        public DateTime Timestamp { get; set; }
        public String Ip { get; set; }
    }
}
