using NuGet.DependencyResolver;
using System;
using System.Collections.Generic;

namespace backendDistributor.Models
{
    public class GRPO
    {
        public Guid Id { get; set; }
        public string? GRPONo { get; set; }
        public string? PurchaseOrderNo { get; set; } // Reference to the original PO
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public DateTime GRPODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? VendorRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? GRPORemarks { get; set; }

        public GRPO()
        {
            GRPOItems = new HashSet<GRPOItem>();
            Attachments = new HashSet<GRPOAttachment>();
        }

        // Relationships
        public ICollection<GRPOItem>? GRPOItems { get; set; }
        public ICollection<GRPOAttachment>? Attachments { get; set; }
    }
}