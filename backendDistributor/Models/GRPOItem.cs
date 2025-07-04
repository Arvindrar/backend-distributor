using System;

namespace backendDistributor.Models
{
    public class GRPOItem
    {
        public Guid Id { get; set; }
        public Guid GRPOId { get; set; } // Foreign Key
        public GRPO? GRPO { get; set; } // Navigation property

        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string? UOM { get; set; }
        public decimal Price { get; set; }
        public string? WarehouseLocation { get; set; }
        public string? TaxCode { get; set; }
        public decimal? TaxPrice { get; set; }
        public decimal Total { get; set; }
    }
}