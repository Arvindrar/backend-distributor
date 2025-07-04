using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace backendDistributor.DTOs
{
    public class GRPOCreateDto
    {
        public string? PurchaseOrderNo { get; set; }
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public DateTime GRPODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? VendorRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? GRPORemarks { get; set; }

        public string? GRPOItemsJson { get; set; }
        public List<IFormFile>? UploadedFiles { get; set; }
        public string? FilesToDeleteJson { get; set; }

        public List<GRPOItemDto>? GRPOItems { get; set; }
    }
}