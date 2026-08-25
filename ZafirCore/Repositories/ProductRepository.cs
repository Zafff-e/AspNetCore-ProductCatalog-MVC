using Microsoft.EntityFrameworkCore;
using AspNetCore_ProductCatalog_MVC.Models;

namespace AspNetCore_ProductCatalog_MVC.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductAsync()
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.Brands)
                                 .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.Brands)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product> SaveProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);  
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task AddBrandByProductIdAsync(int productId, List<Brand> brands)
        {
            if (brands != null && brands.Count > 0)
            {
                foreach (var brand in brands)
                {
                    brand.BrandId = 0;          
                    brand.ProductId = productId;
                    await _context.Brands.AddAsync(brand);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBrandByProductIdAsync(int productId)
        {
            var brands = await _context.Brands
                                       .Where(b => b.ProductId == productId)
                                       .ToListAsync();
            if (brands.Any())
            {
                _context.Brands.RemoveRange(brands);
                await _context.SaveChangesAsync();
            }
        }
    }
}
