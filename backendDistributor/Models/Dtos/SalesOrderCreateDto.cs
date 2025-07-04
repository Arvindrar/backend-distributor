// FILE: DTOs/SalesOrderCreateDto.cs
using System;
using System.Collections.Generic;
using backendDistributor.DTOs;

namespace backendDistributor.DTOs
{
    public class SalesOrderCreateDto
    {
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public DateTime SODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? CustomerRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? SalesRemarks { get; set; }
        public string? SalesEmployee { get; set; }

        public List<SalesItemDto>? SalesItems { get; set; }

        public string? SalesItemsJson { get; set; }

        public List<IFormFile>? UploadedFiles { get; set; }

        // ✅ To support file deletion from frontend
        public string? FilesToDeleteJson { get; set; }
        //public string? RowVersionBase64 { get; set; }


    }
}