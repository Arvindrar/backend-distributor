// backendDistributor/Models/VendorGroup.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class VendorGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vendor Group Name is required.")]
        [StringLength(100, ErrorMessage = "Vendor Group Name cannot be longer than 100 characters.")]
        public string? Name { get; set; }

        // You can add other properties if needed, e.g., Description
        // [StringLength(250)]
        // public string? Description { get; set; }
    }
}