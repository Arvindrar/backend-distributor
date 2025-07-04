// FILE: Models/SalesOrder.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backendDistributor.Models
{
    public class SalesOrder
    {
        public Guid Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public DateTime SODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? CustomerRefNumber { get; set; }
        public string? ShipToAddress { get; set; }
        public string? SalesRemarks { get; set; }
        public string? SalesEmployee { get; set; }

        
        public ICollection<SalesOrderAttachment>? Attachments { get; set; }


        public ICollection<SalesOrderItem>? SalesItems { get; set; }

        

    }
}
