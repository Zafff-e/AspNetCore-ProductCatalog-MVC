using Microsoft.EntityFrameworkCore;

namespace AspNetCore_ProductCatalog_MVC.Models
{
    public class AppDbContext: DbContext
    {
        

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<Category>().HasData(new Category
            { CategoryId = 1, CategoryName = "Electronics" }, new Category
            { CategoryId = 2, CategoryName = "Furniture" }, new Category
            { CategoryId = 3, CategoryName = "Stationary" });
            base.OnModelCreating(modelBuilder);
        }
    }
}
