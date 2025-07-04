namespace backendDistributor.Models
{
    public class GRPONumberTracker
    {
        // Use a different ID to avoid conflict with the PO tracker if they are in the same table
        public int Id { get; set; } = 2; // Primary key, hardcoded to 2
        public int LastUsedNumber { get; set; }
    }
}