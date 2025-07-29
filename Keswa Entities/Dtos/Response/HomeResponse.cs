using Keswa_Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Dtos.Response
{
    public class HomeResponse
    {
        public List<ProductResponse> Products { get; set; }
        public List<CategoryHomeResponse> Categories { get; set; }
        public List<BrandHomeResponse> Brands { get; set; }
    }
}
