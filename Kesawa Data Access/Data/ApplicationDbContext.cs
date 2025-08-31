using Keswa_Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kesawa_Data_Access.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryBrand> CategoryBrands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCart> ProductCarts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductOrder>()
            .HasKey(po => new { po.ProductId, po.OrderId });

            modelBuilder.Entity<ProductCart>()
                .HasKey(pc => new { pc.ProductId, pc.CartId });

            modelBuilder.Entity<CategoryBrand>()
                .HasKey(cb => new { cb.CategoryId, cb.BrandId });

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<Order>()
      .Property(o => o.Status)
      .HasConversion<string>();
            modelBuilder.Entity<Order>()
        .Property(o => o.Status)
        .HasConversion<int>();
        }
    }
}
