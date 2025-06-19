// Models/Dtos/TaxDeclarationDto.cs
namespace backendDistributor.Models.Dtos
{
    public class TaxDeclarationDto
    {
        public int Id { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? IGST { get; set; }
        public decimal TotalPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}