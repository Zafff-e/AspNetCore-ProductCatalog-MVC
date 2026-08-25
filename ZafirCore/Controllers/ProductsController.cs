using Microsoft.AspNetCore.Mvc;
using ZafirCore.Models;
using ZafirCore.Models.ViewModels;
using ZafirCore.Repositories;

namespace ZafirCore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repo;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        
        public async Task<IActionResult> Index()
        {
            var productList = await _repo.GetAllProductAsync();
            return View(productList);
        }

        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel
            {
                Categories = (await _repo.GetAllCategoriesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            ValidateImageFile(model.ProfileFile);

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductName = model.ProductName,
                    ManufacturingDate = model.ManufacturingDate,
                    SerialNo = model.SerialNo,
                    ImageUrl = await GetImageNameAsync(model.ProfileFile),
                    IsAvailable = model.IsAvailable,
                    CategoryId = model.CategoryId,
                    Price = model.Price,
                    Brands = model.Brands != null
                        ? model.Brands.Select(b => new Brand
                        {
                            BrandName = b.BrandName,
                            Stock = b.Stock
                        }).ToList()
                        : new List<Brand>()
                };

                await _repo.SaveProductAsync(product);

                TempData["Success"] = $"Product '{product.ProductName}' was created.";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Products") });
            }

            return Json(new { success = false, errors = GetErrorList() });
        }

        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _repo.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var viewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ManufacturingDate = product.ManufacturingDate,
                SerialNo = product.SerialNo,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable,
                CategoryId = product.CategoryId,
                Price = product.Price,
                Categories = (await _repo.GetAllCategoriesAsync()).ToList(),
                Brands = product.Brands.Select(b => new BrandViewModel
                {
                    BrandId = b.BrandId,
                    BrandName = b.BrandName,
                    Stock = b.Stock
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model, string OldImageUrl)
        {
            ValidateImageFile(model.ProfileFile);

            if (ModelState.IsValid)
            {
                var existing = await _repo.GetProductByIdAsync(model.ProductId);
                if (existing == null) return NotFound();

                if (model.ProfileFile != null)
                {
                    DeleteImageIfExists(OldImageUrl);
                    existing.ImageUrl = await GetImageNameAsync(model.ProfileFile);
                }
                else
                {
                    existing.ImageUrl = OldImageUrl;
                }

                existing.ProductName = model.ProductName;
                existing.ManufacturingDate = model.ManufacturingDate;
                existing.SerialNo = model.SerialNo;
                existing.IsAvailable = model.IsAvailable;
                existing.CategoryId = model.CategoryId;
                existing.Price = model.Price;

                await _repo.UpdateProductAsync(existing);

                await _repo.DeleteBrandByProductIdAsync(model.ProductId);
                if (model.Brands != null && model.Brands.Any())
                {
                    await _repo.AddBrandByProductIdAsync(
                        model.ProductId,
                        model.Brands.Select(b => new Brand
                        {
                            BrandName = b.BrandName,
                            Stock = b.Stock
                        }).ToList());
                }

                TempData["Success"] = $"Product '{existing.ProductName}' was updated.";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Products") });
            }

            return Json(new { success = false, errors = GetErrorList() });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _repo.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            DeleteImageIfExists(product.ImageUrl);
            await _repo.DeleteProductAsync(id);

            TempData["Success"] = $"Product '{product.ProductName}' was deleted.";
            return RedirectToAction("Index");
        }

        private void ValidateImageFile(IFormFile? file)
        {
            if (file == null) return;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
                ModelState.AddModelError("ProfileFile", "Only JPG, PNG, GIF or WEBP files are allowed.");
        }

        private async Task<string> GetImageNameAsync(IFormFile? profileFile)
        {
            if (profileFile == null || profileFile.Length == 0) return "";

            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(profileFile.FileName);
            var uploadFolder = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, imageName);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                                                        FileShare.None, 4096, useAsync: true);
            await profileFile.CopyToAsync(fileStream);

            return imageName;
        }

        private void DeleteImageIfExists(string? imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return;

            var imagePath = Path.Combine(_env.WebRootPath, "images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }

        private object GetErrorList()
        {
            return ModelState
                .Where(x => x.Value!.Errors.Count > 0)
                .Select(x => new
                {
                    field = x.Key,
                    error = x.Value!.Errors.FirstOrDefault()?.ErrorMessage ?? "Value type mismatch"
                })
                .ToList();
        }
    }
}

