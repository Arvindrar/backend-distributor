// Models/UOM.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class UOM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "UOM name is required.")]
        [StringLength(100, ErrorMessage = "UOM name cannot be longer than 100 characters.")]
        public string? Name { get; set; }

        // You can add other properties if needed in the future, e.g.:
        // public string Description { get; set; }
        // public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        // public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}