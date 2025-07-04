using System;

namespace backendDistributor.DTOs
{
    public class GRPOListDto
    {
        public Guid Id { get; set; }
        public string? GRPONo { get; set; }
        public string? PurchaseOrderNo { get; set; }
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public DateTime GRPODate { get; set; }
        public decimal GRPOTotal { get; set; }
        public string? GRPORemarks { get; set; }
    }
}