// FILE: Models/PurchaseOrderAttachment.cs
using System;

namespace backendDistributor.Models
{
    public class PurchaseOrderAttachment
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; } // Foreign Key
        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; } // Navigation property
    }
}