// backendDistributor/Models/SalesEmployee.cs
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Models
{
    public class SalesEmployee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee Code is required.")]
        [StringLength(50, ErrorMessage = "Employee Code cannot be longer than 50 characters.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee Name is required.")]
        [StringLength(150, ErrorMessage = "Employee Name cannot be longer than 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [StringLength(20, ErrorMessage = "Contact Number is too long.")] // Allows for country code like +91
        public string ContactNumber { get; set; } = string.Empty;

        private string? _email;

        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100)]
        public string? Email
        {
            get => _email;
            // This custom setter is the key to the solution.
            // It ensures that if the frontend sends an empty string "",
            // it gets converted to null before validation happens.
            set => _email = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        // Optional: Keep track of active status
        public bool IsActive { get; set; } = true;
    }
}