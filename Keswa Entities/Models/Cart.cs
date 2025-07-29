using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Models
{
    public class Cart
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string MainImage { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public List<ProductCart> ProductCarts { get; set; }
    }
}
