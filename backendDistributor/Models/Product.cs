using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Code is required.")]
        [StringLength(50, ErrorMessage = "Product Code cannot be longer than 50 characters.")]
        public string SKU { get; set; } // Corresponds to formData.productCode

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(150, ErrorMessage = "Product Name cannot be longer than 150 characters.")]
        public string Name { get; set; } // Corresponds to formData.productName

        [Required(ErrorMessage = "Product Group is required.")]
        [StringLength(100)]
        public string Group { get; set; } // Corresponds to formData.productGroup

        // ADDED UOMGroup HERE
        [StringLength(50, ErrorMessage = "UOM Group cannot be longer than 50 characters.")] // Adjust length as needed
        public string? UOMGroup { get; set; } // Corresponds to formData.uomGroup

        [Required(ErrorMessage = "UOM is required.")]
        [StringLength(50, ErrorMessage = "UOM cannot be longer than 50 characters.")]
        public string UOM { get; set; } // Corresponds to formData.uom

        [StringLength(20, ErrorMessage = "HSN cannot be longer than 20 characters.")]
        public string? HSN { get; set; } // Corresponds to formData.hsn

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? RetailPrice { get; set; } // DIRECTLY MATCHES formData.retailPrice

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? WholesalePrice { get; set; } // DIRECTLY MATCHES formData.wholesalePrice

        [StringLength(255)]
        public string? ImageFileName { get; set; } // Stores only the filename from imageFile
    }
}