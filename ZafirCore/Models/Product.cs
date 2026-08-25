using System.ComponentModel.DataAnnotations;

namespace AspNetCore_ProductCatalog_MVC.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;

        [Display(Name = "Manufacturing Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ManufacturingDate { get; set; } = DateTime.Now;

        [Display(Name = "Serial No")]
        public string SerialNo { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public decimal Price { get; set; }

        
        public virtual Category Category { get; set; } = null!;

       
        public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
    }


    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }


    public class Brand
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public int Stock { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }


}
