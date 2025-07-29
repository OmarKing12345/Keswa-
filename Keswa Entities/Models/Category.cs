using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }=string.Empty;
        public bool Status { get; set; }

        public List<Product> Products { get; set; } = null!;

        public List<CategoryBrand> CategoryBrands { get; set; } = null!;
    }
}
