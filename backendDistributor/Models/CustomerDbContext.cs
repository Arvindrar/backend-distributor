using Microsoft.EntityFrameworkCore;

namespace backendDistributor.Models
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
        }

        // --- Original DbSets (assuming these were correct from your project) ---
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorGroup> VendorGroups { get; set; }
        public DbSet<Customer> Customer { get; set; } // Note: Standard convention is Plural (Customers)
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<SalesEmployee> SalesEmployees { get; set; }
        public DbSet<ShippingType> ShippingTypes { get; set; }
        public DbSet<TaxDeclaration> TaxDeclarations { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<UOM> UOMs { get; set; } // This is the DbSet for the UOMsController CS1061 error
        public DbSet<UOMGroup> UOMGroups { get; set; }


        // --- New DbSets for Sales ---
#pragma warning disable CS8618 // Non-nullable field is set by EF Core.
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<SalesOrderAttachment> SalesOrderAttachments { get; set; }
#pragma warning restore CS8618


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configure SalesOrder Relationships ---
            modelBuilder.Entity<SalesOrder>(entity =>
            {
                entity.HasMany(so => so.SalesOrderItems)
                    .WithOne(item => item.SalesOrder)
                    .HasForeignKey(item => item.SalesOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(so => so.Attachments)
                    .WithOne(att => att.SalesOrder)
                    .HasForeignKey(att => att.SalesOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // --- Configure Decimal Precision for SalesOrderItem ---
            modelBuilder.Entity<SalesOrderItem>(entity =>
            {
                // For Quantity: If it can have decimals (e.g., 10.5 units)
                // Adjust precision and scale based on your expected range.
                entity.Property(e => e.Quantity).HasPrecision(18, 4); // Example: 18 total digits, 4 decimal places

                // Price, TaxPrice, and Total in SalesOrderItem.cs already use [Column(TypeName = "decimal(18, 2)")]
                // If you remove those attributes, you would configure precision here:
                // entity.Property(e => e.Price).HasPrecision(18, 2);
                // entity.Property(e => e.TaxPrice).HasPrecision(18, 2);
                // entity.Property(e => e.Total).HasPrecision(18, 2);
            });

            // --- OPTIONAL: Configure Decimal Precision for Product (Example) ---
            // If your Product model has decimal properties like RetailPrice or WholesalePrice,
            // and they don't have [Column(TypeName = "...")] attributes, configure them here.
            /*
            modelBuilder.Entity<Product>(entity =>
            {
                // Assuming Product has these properties and they are decimal
                // entity.Property(p => p.RetailPrice).HasPrecision(18, 2);
                // entity.Property(p => p.WholesalePrice).HasPrecision(18, 2);
            });
            */

            // --- Add any other specific model configurations you had or need ---
            // For example, if you had configurations for Customer, ProductGroup, etc.
        }
    }
}