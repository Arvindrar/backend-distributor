using backendDistributor.Models;
using System;

public class SalesOrderAttachment
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }

    public SalesOrder? SalesOrder { get; set; }
}
