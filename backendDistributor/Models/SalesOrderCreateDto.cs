using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace backendDistributor.DTOs
{
    public class SalesOrderCreateDto
    {
        public string? SalesOrderNo { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? SODate { get; set; } // Receive as string, parse to DateTime
        public string? DeliveryDate { get; set; } // Receive as string, parse to DateTime
        public string? CustomerRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? SalesRemarks { get; set; }
        public string? SalesEmployee { get; set; }

        // This property will receive the JSON string of sales items
        public string? SalesItemsJson { get; set; } // Renamed from salesItems to avoid confusion with IFormFile collection

        // This will bind to the files uploaded with the key "uploadedFiles"
        public List<IFormFile>? UploadedFiles { get; set; }

        // Parsed SalesItems after deserialization
        [System.Text.Json.Serialization.JsonIgnore] // Don't try to bind this directly from form
        public List<SalesOrderItemDto>? ParsedSalesItems { get; set; }
    }

    public class SalesOrderViewDto // For returning sales order details
    {
        public Guid Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? SODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? CustomerRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? SalesRemarks { get; set; }
        public string? SalesEmployee { get; set; }
        public List<SalesOrderItemViewDto> SalesOrderItems { get; set; } = new List<SalesOrderItemViewDto>();
        public List<SalesOrderAttachmentViewDto> Attachments { get; set; } = new List<SalesOrderAttachmentViewDto>(); // For viewing
        public DateTime CreatedDate { get; set; }
        public decimal OrderTotal { get; set; }
    }

    public class SalesOrderItemViewDto
    {
        public Guid Id { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string? UOM { get; set; }
        public decimal Price { get; set; }
        public string? WarehouseLocation { get; set; }
        public string? TaxCode { get; set; }
        public decimal TaxPrice { get; set; }
        public decimal Total { get; set; }
    }

    public class SalesOrderAttachmentViewDto
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        // Potentially a URL to download the file
        public string? DownloadUrl { get; set; }
    }
}