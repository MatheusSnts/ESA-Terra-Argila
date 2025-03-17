using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;


namespace ESA_Terra_Argila.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Material> Materials { get; set; } = default!;
        public DbSet<ProductImage> ProductImages { get; set; } = default!;
        public DbSet<MaterialImage> MaterialImages { get; set; } = default!;
        public DbSet<ProductMaterial> ProductMaterials { get; set; } = default!;
        public DbSet<UserMaterialFavorite> UserMaterialFavorites { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Tag> Tags { get; set; } = default!;
        public DbSet<LogEntry> LogEntries { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.User)
                .WithMany(u => u.Materials)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserMaterialFavorite>()
                .HasOne(umf => umf.User)
                .WithMany(u => u.FavoriteMaterials)
                .HasForeignKey(umf => umf.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMaterialFavorite>()
                .HasOne(umf => umf.Material)
                .WithMany(m => m.FavoritedByUsers)
                .HasForeignKey(umf => umf.MaterialId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });

            modelBuilder.Entity<ProductMaterial>()
                .Property(pm => pm.Stock)
                .HasDefaultValue(0);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .UsingEntity(j => j.ToTable("ProductTags"));

            modelBuilder.Entity<Material>()
                .HasMany(m => m.Tags)
                .WithMany(t => t.Materials)
                .UsingEntity(j => j.ToTable("MaterialTags"));

            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialImage>()
                .HasOne(i => i.Material)
                .WithMany(m => m.Images)
                .HasForeignKey(i => i.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
