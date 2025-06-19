// Add this class to your DTOs file (e.g., SalesOrderDtos.cs or a new SalesOrderUpdateDto.cs)
// If it's in a new file, add:
// using System;
// using System.Collections.Generic;
// using Microsoft.AspNetCore.Http;

namespace backendDistributor.DTOs
{
    // SalesOrderCreateDto, SalesOrderViewDto, SalesOrderItemViewDto, SalesOrderAttachmentViewDto
    // ... (from previous code) ...

    public class SalesOrderUpdateDto
    {
        // No Id needed in the DTO if passed in route, but useful if part of body
        // public Guid Id { get; set; }

        public string? SalesOrderNo { get; set; }
        public string? CustomerCode { get; set; } // You might not allow changing the customer on an existing SO
        public string? CustomerName { get; set; }
        public string? SODate { get; set; }
        public string? DeliveryDate { get; set; }
        public string? CustomerRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? SalesRemarks { get; set; }
        public string? SalesEmployee { get; set; }

        public string? SalesItemsJson { get; set; } // For simplicity, we'll replace all items

        public List<IFormFile>? UploadedFiles { get; set; } // For adding new files
        public List<Guid>? FilesToDelete { get; set; } // IDs of existing attachments to delete

        [System.Text.Json.Serialization.JsonIgnore]
        public List<SalesOrderItemDto>? ParsedSalesItems { get; set; }
    }
}