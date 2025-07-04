namespace backendDistributor.Models
{
    public class SalesOrderNumberTracker
    {
        public int Id { get; set; } = 1;
        public int LastUsedNumber { get; set; }
    }
}
