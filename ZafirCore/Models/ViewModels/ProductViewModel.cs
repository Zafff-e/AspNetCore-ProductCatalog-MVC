using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ZafirCore.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be 3–100 characters.")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = null!;

        [Required(ErrorMessage = "Manufacturing date is required.")]
        
        [Display(Name = "Manufacturing Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ManufacturingDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Serial number is required.")]
        [RegularExpression(@"^[A-Z]{2}-\d{4,8}$",
            ErrorMessage = "Serial must look like AB-12345 (two capitals, dash, 4–8 digits).")]
        [Display(Name = "Serial No")]
        public string SerialNo { get; set; } = null!;

        [ValidateNever]
        public string? ImageUrl { get; set; }

        [ValidateNever]
        [DataType(DataType.Upload)]
        [Display(Name = "Product Image")]
        public IFormFile? ProfileFile { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 9999999.99, ErrorMessage = "Price must be between 0.01 and 9,999,999.99.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [ValidateNever]
        public virtual Category? Category { get; set; }

        
        [ValidateNever]
        public List<Category>? Categories { get; set; }

       
        public virtual IList<BrandViewModel> Brands { get; set; } = new List<BrandViewModel>();
    }
}
