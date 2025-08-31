using Keswa_Entities.Models.Emum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa_Entities.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string TrackingCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        public string StripeSessionId { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
    }



}
