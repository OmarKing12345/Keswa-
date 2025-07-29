using Keswa_Entities.Models;
using Keswa_Project.Keswa_Entities.Dtos.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Dtos.Response
{
    public class ProductResponse
    {
      
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<ProductImageResponse> ProductImages { get; set; }=null!;

        public double Price { get; set; }
        public int Count { get; set; }
        public int Views { get; set; }
        public bool Status { get; set; }
 
        public int BrandId { get; set; }
        public string? BrandName { get; set; }  

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }  

    }
}
