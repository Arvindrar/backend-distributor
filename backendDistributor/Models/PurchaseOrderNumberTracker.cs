// FILE: Models/PurchaseOrderNumberTracker.cs
namespace backendDistributor.Models
{
    public class PurchaseOrderNumberTracker
    {
        public int Id { get; set; } = 1; // Primary key, hardcoded to 1
        public int LastUsedNumber { get; set; }
    }
}