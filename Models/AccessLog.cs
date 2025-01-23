using System;

namespace ESA_Terra_Argila.Models
{
    public class AccessLog
    {
        public int Id { get; set; }
        public string? IP { get; set; } // Permite valores nulos
        public string? Path { get; set; }
        public string? Method { get; set; }
        public DateTime Timestamp { get; set; }
      
    }
}
