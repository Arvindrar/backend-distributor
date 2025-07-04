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
        public DbSet<Customer> Customers { get; set; } // Note: Standard convention is Plural (Customers)
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<SalesEmployee> SalesEmployees { get; set; }
        public DbSet<ShippingType> ShippingTypes { get; set; }
        public DbSet<TaxDeclaration> TaxDeclarations { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<UOM> UOMs { get; set; } // This is the DbSet for the UOMsController CS1061 error
        public DbSet<UOMGroup> UOMGroups { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderNumberTracker> SalesOrderNumberTrackers { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<SalesOrderAttachment> SalesOrderAttachments { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseOrderAttachment> PurchaseOrderAttachments { get; set; }
        public DbSet<PurchaseOrderNumberTracker> PurchaseOrderNumberTrackers { get; set; }

        public DbSet<GRPO> GRPOs { get; set; }
        public DbSet<GRPOItem> GRPOItems { get; set; }
        public DbSet<GRPOAttachment> GRPOAttachments { get; set; }
        public DbSet<GRPONumberTracker> GRPONumberTrackers { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ✅ Fix table name explicitly
            //modelBuilder.Entity<Customer>().ToTable("Customer");

            modelBuilder.Entity<SalesOrderItem>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.TaxPrice).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
            });

            modelBuilder.Entity<SalesOrderItem>()
               .HasOne(i => i.SalesOrder)
               .WithMany(o => o.SalesItems)
               .HasForeignKey(i => i.SalesOrderId);

            modelBuilder.Entity<SalesOrderNumberTracker>().HasData(
        new SalesOrderNumberTracker { Id = 1, LastUsedNumber = 1000000 }
    );
            modelBuilder.Entity<PurchaseOrderNumberTracker>().HasData(new PurchaseOrderNumberTracker { Id = 1, LastUsedNumber = 1000000 });

            modelBuilder.Entity<GRPOItem>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.TaxPrice).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
            });

            // 2. Configure the one-to-many relationship for GRPO and GRPOItem
            modelBuilder.Entity<GRPOItem>()
               .HasOne(i => i.GRPO)
               .WithMany(g => g.GRPOItems)
               .HasForeignKey(i => i.GRPOId);

            // 3. Configure the one-to-many relationship for GRPO and GRPOAttachment
            modelBuilder.Entity<GRPOAttachment>()
               .HasOne(a => a.GRPO)
               .WithMany(g => g.Attachments)
               .HasForeignKey(a => a.GRPOId);

            // 4. Seed the GRPONumberTracker
            modelBuilder.Entity<GRPONumberTracker>().HasData(
                new GRPONumberTracker { Id = 2, LastUsedNumber = 1000000 }
            );

        }
    }
}