using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Dtos.Request
{
    public class HomeRequest
    {
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
    }
}
