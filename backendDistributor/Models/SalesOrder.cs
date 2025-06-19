using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class SalesOrder
    {
        [Key]
        public Guid Id { get; set; } // Using GUID for unique ID

        [StringLength(50)]
        public string? SalesOrderNo { get; set; } // SO Number, might be auto-generated or user-input

        // Foreign Key for Customer
        public int? CustomerId { get; set; } // Assuming CustomerId is int
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; } // Navigation property

        // Direct storage of customer details at the time of order creation (denormalization for historical accuracy)
        [StringLength(100)]
        public string? CustomerCode { get; set; } // Could be from Customer.Code

        [StringLength(255)]
        public string? CustomerName { get; set; } // Could be from Customer.Name

        public DateTime? SODate { get; set; }
        public DateTime? DeliveryDate { get; set; } // Due Date

        [StringLength(500)]
        public string? DocumentDetails { get; set; } // Not used in frontend, but good to have

        [StringLength(100)]
        public string? CustomerRefNumber { get; set; }

        [StringLength(500)]
        public string? ShipToAddress { get; set; } // Bill to Address

        [StringLength(1000)]
        public string? SalesRemarks { get; set; }

        [StringLength(100)]
        public string? SalesEmployee { get; set; } // Could be a FK to SalesEmployee table later

        // For file attachments - store paths or references
        // This is a simple representation; you might use a separate table for multiple attachments per order
        // public string? AttachmentFileNames { get; set; } // e.g., "file1.pdf,file2.jpg" or JSON array as string

        public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
        public virtual ICollection<SalesOrderAttachment> Attachments { get; set; } = new List<SalesOrderAttachment>();


        // Calculated Totals (not directly mapped to DB if calculated on the fly,
        // or can be stored for performance if frequently queried)
        [NotMapped] // If calculated in application layer
        public decimal ProductTotalSummary { get; set; }
        [NotMapped]
        public decimal TaxTotalSummary { get; set; }
        [NotMapped]
        public decimal GrandTotalSummary { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }

    public class SalesOrderAttachment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SalesOrderId { get; set; }
        [ForeignKey("SalesOrderId")]
        public virtual SalesOrder? SalesOrder { get; set; }

        [Required]
        [StringLength(255)]
        public string? FileName { get; set; } // Original file name

        [Required]
        [StringLength(1024)]
        public string? StoredFileName { get; set; } // Name used to store on server (e.g., GUID.ext)

        [Required]
        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    }
}