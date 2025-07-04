// FILE: Models/SalesOrderItem.cs
using System;

namespace backendDistributor.Models
{
    public class SalesOrderItem
    {
        public Guid Id { get; set; }
        public Guid SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }

        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; }
        public decimal Price { get; set; }
        public string WarehouseLocation { get; set; }
        public string? TaxCode { get; set; }
        public decimal? TaxPrice { get; set; }
        public decimal Total { get; set; }
    }
}
