// backendDistributor/Models/CustomerGroup.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class CustomerGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer Group Name is required.")]
        [StringLength(100, ErrorMessage = "Customer Group Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        // You can add other properties if needed, e.g., Description
        // [StringLength(250)]
        // public string? Description { get; set; }
    }
}