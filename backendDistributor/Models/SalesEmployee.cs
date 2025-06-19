// backendDistributor/Models/SalesEmployee.cs
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Models
{
    public class SalesEmployee
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Sales employee name is required.")]
        [StringLength(150, ErrorMessage = "Sales employee name cannot be longer than 150 characters.")]
        public string? Name { get; set; }

        // You can add other properties specific to a sales employee
        // For example:
        // public string? EmployeeCode { get; set; }
        // public string? Email { get; set; }
        // public string? PhoneNumber { get; set; }
        // public bool IsActive { get; set; } = true;
    }
}