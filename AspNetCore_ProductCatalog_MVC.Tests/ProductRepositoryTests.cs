using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AspNetCore_ProductCatalog_MVC.Models;
using AspNetCore_ProductCatalog_MVC.Repositories;

namespace AspNetCore_ProductCatalog_MVC.Tests
{
    public class ProductRepositoryTests
    {
        private AppDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAllProductAsync_ReturnsProductsWithCategoriesAndBrands()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var category = new Category { CategoryId = 10, CategoryName = "Hardware" };
            context.Categories.Add(category);

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Gaming Laptop",
                SerialNo = "GL-1001",
                ImageUrl = "laptop.jpg",
                Price = 1200.00m,
                CategoryId = 10,
                Brands = new List<Brand>
                {
                    new Brand { BrandId = 1, BrandName = "Asus", Stock = 15 }
                }
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var results = (await repo.GetAllProductAsync()).ToList();

            Assert.Single(results);
            Assert.Equal("Gaming Laptop", results[0].ProductName);
            Assert.Equal("Hardware", results[0].Category.CategoryName);
            Assert.Single(results[0].Brands);
            Assert.Equal("Asus", results[0].Brands.First().BrandName);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenExists()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var product = new Product
            {
                ProductId = 42,
                ProductName = "Wireless Mouse",
                SerialNo = "WM-42",
                ImageUrl = "mouse.jpg",
                Price = 25.50m,
                CategoryId = 1
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetProductByIdAsync(42);

            Assert.NotNull(result);
            Assert.Equal(42, result.ProductId);
            Assert.Equal("Wireless Mouse", result.ProductName);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsNull_WhenNotExists()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repo = new ProductRepository(context);

            var result = await repo.GetProductByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveProductAsync_PersistsNewProduct()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repo = new ProductRepository(context);

            var product = new Product
            {
                ProductName = "Mechanical Keyboard",
                SerialNo = "MK-500",
                ImageUrl = "keyboard.png",
                Price = 85.00m,
                CategoryId = 1
            };

            var saved = await repo.SaveProductAsync(product);
            Assert.True(saved.ProductId > 0);

            var inDb = await context.Products.FindAsync(saved.ProductId);
            Assert.NotNull(inDb);
            Assert.Equal("Mechanical Keyboard", inDb.ProductName);
        }

        [Fact]
        public async Task UpdateProductAsync_ModifiesFields()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var product = new Product
            {
                ProductId = 5,
                ProductName = "Original Monitor",
                SerialNo = "MON-100",
                ImageUrl = "mon.jpg",
                Price = 200.00m,
                CategoryId = 1
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            product.ProductName = "UltraWide Monitor";
            product.Price = 350.00m;

            var updated = await repo.UpdateProductAsync(product);
            Assert.NotNull(updated);
            Assert.Equal("UltraWide Monitor", updated.ProductName);
            Assert.Equal(350.00m, updated.Price);
        }

        [Fact]
        public async Task DeleteProductAsync_RemovesProduct()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var product = new Product
            {
                ProductId = 10,
                ProductName = "USB Cable",
                SerialNo = "USB-1",
                ImageUrl = "usb.jpg",
                Price = 9.99m,
                CategoryId = 1
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var deleted = await repo.DeleteProductAsync(10);

            Assert.NotNull(deleted);
            Assert.Null(await context.Products.FindAsync(10));
        }

        [Fact]
        public async Task AddBrandByProductIdAsync_AssociatesBrandsWithParentProduct()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var product = new Product
            {
                ProductId = 15,
                ProductName = "Headset",
                SerialNo = "HS-9",
                ImageUrl = "hs.jpg",
                Price = 45.00m,
                CategoryId = 1
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var brands = new List<Brand>
            {
                new Brand { BrandName = "Sony", Stock = 20 },
                new Brand { BrandName = "Bose", Stock = 10 }
            };

            await repo.AddBrandByProductIdAsync(15, brands);

            var inDbBrands = await context.Brands.Where(b => b.ProductId == 15).ToListAsync();
            Assert.Equal(2, inDbBrands.Count);
            Assert.All(inDbBrands, b => Assert.Equal(15, b.ProductId));
        }

        [Fact]
        public async Task DeleteBrandByProductIdAsync_DeletesOnlySpecificParentBrands()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            context.Brands.AddRange(
                new Brand { BrandId = 101, BrandName = "Brand A", Stock = 5, ProductId = 1 },
                new Brand { BrandId = 102, BrandName = "Brand B", Stock = 10, ProductId = 1 },
                new Brand { BrandId = 103, BrandName = "Brand C", Stock = 15, ProductId = 2 }
            );
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            await repo.DeleteBrandByProductIdAsync(1);

            var p1Brands = await context.Brands.Where(b => b.ProductId == 1).ToListAsync();
            var p2Brands = await context.Brands.Where(b => b.ProductId == 2).ToListAsync();

            Assert.Empty(p1Brands);
            Assert.Single(p2Brands);
            Assert.Equal("Brand C", p2Brands[0].BrandName);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsCategories()
        {
            var context = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repo = new ProductRepository(context);

            var categories = await repo.GetAllCategoriesAsync();
            Assert.NotEmpty(categories);
        }
    }
}
