// backendDistributor/Models/ShippingType.cs
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Models
{
    public class ShippingType
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Shipping type name is required.")]
        [StringLength(100, ErrorMessage = "Shipping type name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        // You can add other properties if needed, e.g.:
        // public string? Description { get; set; }
        // public bool IsActive { get; set; } = true;
    }
}