// Models/Dtos/TaxDeclarationCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Models.Dtos
{
    public class TaxDeclarationCreateDto
    {
        [Required(ErrorMessage = "Tax Code is required.")]
        [StringLength(50, ErrorMessage = "Tax Code cannot be longer than 50 characters.")]
        public string TaxCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax Description is required.")]
        [StringLength(255, ErrorMessage = "Tax Description cannot be longer than 255 characters.")]
        public string TaxDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valid From date is required.")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Valid To date is required.")]
        public DateTime ValidTo { get; set; }

        [Range(0, 999.99, ErrorMessage = "CGST must be between 0 and 999.99.")]
        public decimal? CGST { get; set; }

        [Range(0, 999.99, ErrorMessage = "SGST must be between 0 and 999.99.")]
        public decimal? SGST { get; set; }

        [Range(0, 999.99, ErrorMessage = "IGST must be between 0 and 999.99.")]
        public decimal? IGST { get; set; }

        [Required(ErrorMessage = "Total Percentage is required.")]
        [Range(0, 999.99, ErrorMessage = "Total Percentage must be between 0 and 999.99.")]
        public decimal TotalPercentage { get; set; }

        // IsActive will default to true on creation or be handled by the server.
    }
}