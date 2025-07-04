// FILE: DTOs/PurchaseOrderListDto.cs
using System;

namespace backendDistributor.DTOs
{
    public class PurchaseOrderListDto
    {
        public Guid Id { get; set; }
        public string? PurchaseOrderNo { get; set; }
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public DateTime PODate { get; set; }
        public string? PurchaseRemarks { get; set; }
        public decimal OrderTotal { get; set; }
    }
}