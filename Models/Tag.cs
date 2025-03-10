using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESA_Terra_Argila.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo Referência é obrigatório.")]
        [StringLength(50, ErrorMessage = "A Referência deve ter no máximo 50 caracteres.")]
        [Display(Name = "Código de Referência")]
        public string Reference { get; set; } = default!;

        public string? UserId { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da Tag")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "O campo Visibilidade é obrigatório.")]
        [Display(Name = "Visibilidade")]
        public bool IsPublic { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProductTag> ProductTags { get; set; }

        [JsonIgnore]
        public virtual ICollection<MaterialTag> MaterialTags { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public Tag()
        {
            ProductTags = new HashSet<ProductTag>();
            MaterialTags = new HashSet<MaterialTag>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
