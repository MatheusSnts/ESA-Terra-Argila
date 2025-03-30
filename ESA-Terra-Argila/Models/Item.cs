using ESA_Terra_Argila.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public abstract class Item
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = default!;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = default!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }

        [Range(0, double.MaxValue)]
        public float Stock { get; set; } = 0;

        [Required(ErrorMessage = "Escolha uma unidade.")]
        [Range(1, int.MaxValue, ErrorMessage = "Escolha uma unidade válida.")]
        public UnitType Unit { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<ItemImage> Images { get; set; }

        protected Item()
        {
            CreatedAt = DateTime.UtcNow;
            Images = new HashSet<ItemImage>();
        }
    }
}
