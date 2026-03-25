using ASC.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }
        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Product> Products { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MasterDataKey>()
                .HasKey(e => new { e.PartitionKey, e.RowKey });

            builder.Entity<MasterDataValue>()
                .HasKey(e => new { e.PartitionKey, e.RowKey });

            builder.Entity<ServiceRequest>()
                .HasKey(e => new { e.PartitionKey, e.RowKey });

            builder.Entity<Product>()
                .HasKey(e => e.ProductId);

            base.OnModelCreating(builder);
        }
    }
}