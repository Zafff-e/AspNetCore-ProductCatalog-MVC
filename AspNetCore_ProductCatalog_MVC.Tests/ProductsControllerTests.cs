using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;
using AspNetCore_ProductCatalog_MVC.Controllers;
using AspNetCore_ProductCatalog_MVC.Models;
using AspNetCore_ProductCatalog_MVC.Models.ViewModels;
using AspNetCore_ProductCatalog_MVC.Repositories;

namespace AspNetCore_ProductCatalog_MVC.Tests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public ProductsControllerTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns("C:\\FakeRoot");
        }

        private ProductsController CreateControllerWithContext()
        {
            var controller = new ProductsController(_mockRepo.Object, _mockEnv.Object);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/Products/Index");
            controller.Url = urlHelper.Object;

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfProducts()
        {
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Product A" },
                new Product { ProductId = 2, ProductName = "Product B" }
            };
            _mockRepo.Setup(r => r.GetAllProductAsync()).ReturnsAsync(products);

            var controller = CreateControllerWithContext();
            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Create_Get_ReturnsView_WithCategories()
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Electronics" },
                new Category { CategoryId = 2, CategoryName = "Appliances" }
            };
            _mockRepo.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(categories);

            var controller = CreateControllerWithContext();
            var result = await controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductViewModel>(viewResult.Model);
            Assert.Equal(2, model.Categories.Count);
        }

        [Fact]
        public async Task Create_Post_ValidModel_SavesProductAndReturnsJsonSuccess()
        {
            var model = new ProductViewModel
            {
                ProductName = "New Gizmo",
                SerialNo = "GZ-1",
                Price = 99.99m,
                CategoryId = 1,
                Brands = new List<BrandViewModel>
                {
                    new BrandViewModel { BrandName = "GizmoCorp", Stock = 50 }
                }
            };

            _mockRepo.Setup(r => r.SaveProductAsync(It.IsAny<Product>()))
                     .ReturnsAsync((Product p) => { p.ProductId = 100; return p; });

            var controller = CreateControllerWithContext();
            var result = await controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            _mockRepo.Verify(r => r.SaveProductAsync(It.Is<Product>(p => p.ProductName == "New Gizmo")), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsJsonFailure()
        {
            var model = new ProductViewModel();
            var controller = CreateControllerWithContext();
            controller.ModelState.AddModelError("ProductName", "Product Name is required");

            var result = await controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            _mockRepo.Verify(r => r.SaveProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_NonExistentProduct_ReturnsNotFound()
        {
            _mockRepo.Setup(r => r.GetProductByIdAsync(999)).ReturnsAsync((Product?)null);

            var controller = CreateControllerWithContext();
            var result = await controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ExistingProduct_ReturnsViewWithProductViewModel()
        {
            var product = new Product
            {
                ProductId = 7,
                ProductName = "Existing Widget",
                SerialNo = "EW-7",
                CategoryId = 2,
                Price = 19.99m,
                Brands = new List<Brand>
                {
                    new Brand { BrandId = 1, BrandName = "WidgetCo", Stock = 10 }
                }
            };
            var categories = new List<Category> { new Category { CategoryId = 2, CategoryName = "Hardware" } };

            _mockRepo.Setup(r => r.GetProductByIdAsync(7)).ReturnsAsync(product);
            _mockRepo.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(categories);

            var controller = CreateControllerWithContext();
            var result = await controller.Edit(7);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductViewModel>(viewResult.Model);
            Assert.Equal(7, model.ProductId);
            Assert.Equal("Existing Widget", model.ProductName);
            Assert.Single(model.Brands);
        }

        [Fact]
        public async Task Delete_Post_ExistingProduct_DeletesAndRedirects()
        {
            var product = new Product { ProductId = 3, ProductName = "Doomed Product", ImageUrl = "" };
            _mockRepo.Setup(r => r.GetProductByIdAsync(3)).ReturnsAsync(product);
            _mockRepo.Setup(r => r.DeleteProductAsync(3)).ReturnsAsync(product);

            var controller = CreateControllerWithContext();
            var result = await controller.Delete(3);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockRepo.Verify(r => r.DeleteProductAsync(3), Times.Once);
        }

        [Fact]
        public async Task Delete_Post_NonExistentProduct_ReturnsNotFound()
        {
            _mockRepo.Setup(r => r.GetProductByIdAsync(999)).ReturnsAsync((Product?)null);

            var controller = CreateControllerWithContext();
            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
            _mockRepo.Verify(r => r.DeleteProductAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
