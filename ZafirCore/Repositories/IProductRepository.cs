using ZafirCore.Models;

namespace ZafirCore.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> SaveProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Product product);
        Task<Product?> DeleteProductAsync(int id);

        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task AddBrandByProductIdAsync(int productId, List<Brand> brands);
        Task DeleteBrandByProductIdAsync(int productId);
    }
}
