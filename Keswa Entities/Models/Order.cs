using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool  Status { get; set; }
        public double Price { get; set; }
        public int SessionId { get; set; }
        public int PaymentId { get; set; }
        public DateTime ShippedDate { get; set; }
        public int CarrierId { get; set; }
        public Carrier Carrier { get; set; }

        public List<ProductOrder> ProductOrders { get; set; }

    }
}
