// Models/UOMGroup.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Required for ICollection if you link UOMs

namespace backendDistributor.Models
{
    public class UOMGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "UOM Group name is required.")]
        [StringLength(100, ErrorMessage = "UOM Group name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        // Optional: If UOMs belong to a UOMGroup, you might have a navigation property
        // public virtual ICollection<UOM> UOMs { get; set; } = new List<UOM>();

        // public string Description { get; set; }
        // public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}