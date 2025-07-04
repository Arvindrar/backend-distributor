// Models/Warehouse.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Warehouse code is required.")]
        [StringLength(50, ErrorMessage = "Warehouse code cannot be longer than 50 characters.")]
        public string? Code { get; set; } // e.g., WH-MAIN, NORTH-01

        [Required(ErrorMessage = "Warehouse name is required.")]
        [StringLength(100, ErrorMessage = "Warehouse name cannot be longer than 100 characters.")]
        public string? Name { get; set; } // e.g., Main Warehouse, North Storage

        [Required(ErrorMessage = "Warehouse address is required.")]
        [StringLength(500, ErrorMessage = "Warehouse address cannot be longer than 500 characters.")]
        public string? Address { get; set; } // Descriptive address

        // Optional: Add other properties if needed
        // public bool IsActive { get; set; } = true;
        // public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        // public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}