using Microsoft.EntityFrameworkCore;
//using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESA_Terra_Argila.Models
{
    public class Product
    {
        public int Id { get; set; }

        public required string Reference { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public required decimal Price { get; set; }
    }
}
