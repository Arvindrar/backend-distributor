using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class SalesOrderItem
    {
        [Key]
        public Guid Id { get; set; } // Using GUID

        // Foreign Key to SalesOrder
        public Guid SalesOrderId { get; set; }
        [ForeignKey("SalesOrderId")]
        public virtual SalesOrder? SalesOrder { get; set; }

        // Foreign Key to Product (optional, if you want strict referential integrity)
        // If ProductId is not always known or you allow non-catalog items, this might be nullable or omitted
        public int? ProductId { get; set; } // Assuming ProductId is int in Product model
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } // Navigation property

        // Denormalized product details at the time of order
        [StringLength(50)]
        public string? ProductCode { get; set; } // SKU

        [StringLength(255)]
        public string? ProductName { get; set; }

        [Column(TypeName = "decimal(18, 4)")] // Example: 18 total digits, 4 decimal places for quantity
        public decimal Quantity { get; set; }

        [StringLength(20)]
        public string? UOM { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } // Unit Price

        [StringLength(100)]
        public string? WarehouseLocation { get; set; } // Warehouse

        [StringLength(50)]
        public string? TaxCode { get; set; } // Reference to a tax rule/code

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxPrice { get; set; } // Total tax amount for this line item

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; } // (Quantity * Price) + TaxPrice
    }
}