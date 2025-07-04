// FILE: Models/PurchaseOrder.cs
using System;
using System.Collections.Generic;

namespace backendDistributor.Models
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public string? PurchaseOrderNo { get; set; } // Renamed
        public string? VendorCode { get; set; }      // Renamed
        public string? VendorName { get; set; }      // Renamed
        public DateTime PODate { get; set; }        // Renamed
        public DateTime? DeliveryDate { get; set; }
        public string? VendorRefNumber { get; set; } // Renamed
        public string? ShipToAddress { get; set; }
        public string? PurchaseRemarks { get; set; } // Renamed

        public PurchaseOrder()
        {
            PurchaseItems = new HashSet<PurchaseOrderItem>();
            Attachments = new HashSet<PurchaseOrderAttachment>();
        }

        // Relationships
        public ICollection<PurchaseOrderItem>? PurchaseItems { get; set; }
        public ICollection<PurchaseOrderAttachment>? Attachments { get; set; }
    }
}