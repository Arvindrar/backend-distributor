// Models/TaxDeclaration.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class TaxDeclaration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TaxCode { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string TaxDescription { get; set; } = string.Empty;

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Column(TypeName = "decimal(5, 2)")] // e.g., 999.99
        public decimal? CGST { get; set; } // Nullable, as it might not be applicable

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? SGST { get; set; } // Nullable

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? IGST { get; set; } // Nullable

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal TotalPercentage { get; set; } // Renamed from "TOTAL" to avoid potential keyword issues and be more descriptive

        public bool IsActive { get; set; } = true; // Default to active
    }
}