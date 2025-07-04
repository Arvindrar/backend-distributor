// FILE: DTOs/PurchaseOrderCreateDto.cs
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace backendDistributor.DTOs
{
    public class PurchaseOrderCreateDto
    {
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public DateTime PODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? VendorRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? PurchaseRemarks { get; set; }

        // These properties will be populated from the multipart/form-data
        public string? PurchaseItemsJson { get; set; }
        public List<IFormFile>? UploadedFiles { get; set; }
        public string? FilesToDeleteJson { get; set; }

        // This will be deserialized from PurchaseItemsJson in the controller
        public List<PurchaseItemDto>? PurchaseItems { get; set; }
    }
}