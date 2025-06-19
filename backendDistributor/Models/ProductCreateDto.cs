using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Dtos
{
    public class ProductCreateDto
    {
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } // Matches 'SKU' key in FormData

        [Required]
        [StringLength(250)]
        public string ProductName { get; set; } // Matches 'ProductName' key in FormData

        [Required]
        [StringLength(100)]
        public string ProductGroup { get; set; } // Matches 'ProductGroup' key in FormData

        [StringLength(50)]
        public string? UOMGroup { get; set; } // Matches 'UOMGroup' key in FormData (optional)

        [Required]
        [StringLength(50)]
        public string UOM { get; set; } // Matches 'UOM' key in FormData

        [StringLength(20)]
        public string? HSN { get; set; } // Matches 'HSN' key in FormData

        // These will receive string values from FormData
        public string? RetailPrice { get; set; }    // Matches 'RetailPrice' key in FormData
        public string? WholesalePrice { get; set; } // Matches 'WholesalePrice' key in FormData

        public IFormFile? ProductImage { get; set; } // Matches 'ProductImage' key in FormData
    }
}