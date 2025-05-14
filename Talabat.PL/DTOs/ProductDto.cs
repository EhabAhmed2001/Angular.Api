using System.ComponentModel.DataAnnotations;
using Talabat.Core.Entities;

namespace Talabat.PL.DTOs
{
    public class ProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Image is required.")]
        public IFormFile Picture { get; set; }
        public decimal Price { get; set; }
        public int BrandId { get; set; }
        public int TypeId { get; set; }
    }
}
