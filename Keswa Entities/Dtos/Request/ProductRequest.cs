using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keswa_Entities.Models;
using Microsoft.AspNetCore.Http;


namespace Keswa_Entities.Dtos.Request
{
    public class ProductRequest
    {
        public string Name { get; set; }=string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }

        public int Views { get; set; }

        public List<IFormFile> ProductImages { get; set; } = new();

        public int CategoryId { get; set; }
        public int BrandId { get; set; }
    }
}
