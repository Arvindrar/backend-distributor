// backendDistributor/Models/Route.cs
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Models
{
    public class Route
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Route name is required.")]
        [StringLength(100, ErrorMessage = "Route name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        // You can add other properties here if a Route has more details
        // For example:
        // public string? Description { get; set; }
        // public bool IsActive { get; set; } = true;
    }
}