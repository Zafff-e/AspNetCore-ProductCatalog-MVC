using System.ComponentModel.DataAnnotations;

namespace ZafirCore.Models.ViewModels
{
    public class BrandViewModel
    {
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Brand name must be 2–60 characters.")]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = null!;

        [Range(0, 100000, ErrorMessage = "Stock must be between 0 and 100000.")]
        public int Stock { get; set; }
    }
}
