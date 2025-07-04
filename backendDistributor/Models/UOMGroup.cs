// Models/UOMGroup.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    /// <summary>
    /// Represents a group for Units of Measure (e.g., Weight, Volume, Length).
    /// </summary>
    public class UOMGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "UOM Group name is required.")]
        [StringLength(100, ErrorMessage = "UOM Group name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot be longer than 250 characters.")]
        public string? Description { get; set; } // Nullable string for optional description

        public DateTime CreatedDate { get; set; }

        // Constructor to set the creation date automatically
        public UOMGroup()
        {
            Name = string.Empty; // Initialize to prevent null reference on Name
            CreatedDate = DateTime.UtcNow;
        }
    }
}