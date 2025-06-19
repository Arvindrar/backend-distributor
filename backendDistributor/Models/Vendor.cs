// backendDistributor/Models/Vendor.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class Vendor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vendor Code is required.")]
        [StringLength(50, ErrorMessage = "Vendor Code cannot be longer than 50 characters.")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Vendor Name is required.")]
        [StringLength(200, ErrorMessage = "Vendor Name cannot be longer than 200 characters.")] // Increased from 100 to match Customer
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "Vendor Group cannot be longer than 100 characters.")]
        public string? Group { get; set; } // This will be the name of the VendorGroup

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot be longer than 500 characters.")]
        public string? Remarks { get; set; }

        // Ensuring ContactNumber is nullable to allow empty submissions if not required,
        // but if provided, it must be 10 digits.
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Contact Number must be 10 digits if provided.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact Number must contain only digits and be exactly 10 digits long.")]
        public string? ContactNumber { get; set; }


        [Required(ErrorMessage = "Mail ID is required.")] // Made Mail ID required as per form
        [StringLength(100, ErrorMessage = "Mail ID cannot be longer than 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")] // Simpler email validation
        // More specific regex if needed: [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email Address format. It must include a domain (e.g., example.com).")]
        public string? MailId { get; set; }


        [Required(ErrorMessage = "Shipping Type is required.")] // Made Shipping Type required as per form
        [StringLength(50, ErrorMessage = "Shipping Type cannot be longer than 50 characters.")]
        public string? ShippingType { get; set; }

        // Address Fields
        [StringLength(100, ErrorMessage = "Address1 cannot be longer than 100 characters.")]
        public string? Address1 { get; set; }

        [StringLength(100, ErrorMessage = "Address2 cannot be longer than 100 characters.")]
        public string? Address2 { get; set; }

        [StringLength(100, ErrorMessage = "Street cannot be longer than 100 characters.")]
        public string? Street { get; set; }

        // Ensuring PostBox is nullable, but if provided, must be 6 digits.
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Post Box must be 6 digits if provided.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Post Box must contain only digits and be exactly 6 digits long.")]
        public string? PostBox { get; set; }


        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot be longer than 100 characters.")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters.")]
        public string? Country { get; set; }

        // Ensuring GSTIN is nullable, but if provided, must be 15 characters.
        // The regex ensures it's alphanumeric, common for GSTINs. Adjust if needed.
        [StringLength(15, MinimumLength = 15, ErrorMessage = "GSTIN must be 15 characters if provided.")]
        [RegularExpression(@"^[A-Z0-9]{15}$", ErrorMessage = "GSTIN must be 15 alphanumeric characters (uppercase).")] // Example: 22AAAAA0000A1Z5
        public string? Gstin { get; set; }
    }
}