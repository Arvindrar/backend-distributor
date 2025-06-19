using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendDistributor.Models
{
    public class ProductGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product group name is required.")]
        [StringLength(100, ErrorMessage = "Product group name cannot be longer than 100 characters.")]
        public string Name { get; set; }
    }
}