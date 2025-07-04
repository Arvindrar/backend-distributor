// FILE: DTOs/PurchaseItemDto.cs
namespace backendDistributor.DTOs
{
    public class PurchaseItemDto
    {
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