using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Models;


namespace ESA_Terra_Argila.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Material> Materials { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Tag> Tags { get; set; } = default!;
        public DbSet<ProductMaterial> ProductMaterials { get; set; } = default!;
        public DbSet<ProductTag> ProductTags { get; set; } = default!;
        public DbSet<MaterialTag> MaterialTags { get; set; } = default!;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CategoryId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId);

            modelBuilder.Entity<ProductTag>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId);

            modelBuilder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId);

            modelBuilder.Entity<MaterialTag>()
                .HasOne(mt => mt.Material)
                .WithMany(m => m.MaterialTags)
                .HasForeignKey(mt => mt.MaterialId);

            modelBuilder.Entity<MaterialTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.MaterialTags)
                .HasForeignKey(mt => mt.TagId);
        }
    }
}
