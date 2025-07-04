namespace backendDistributor.Models.DTOs
{
    public class SalesOrderListDto
    {
        public Guid Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public DateTime SODate { get; set; }
        public string? SalesRemarks { get; set; }
        public decimal OrderTotal { get; set; } // NEW
    }

}
