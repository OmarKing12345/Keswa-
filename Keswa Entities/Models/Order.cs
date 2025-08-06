using Keswa_Entities.Models.Emum;

namespace Keswa_Entities.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // مجموع المبلغ الكلي للطلب
        public decimal TotalAmount { get; set; }

        // حالة الطلب (Pending, Paid, Shipped...)
        public OrderStatus Status { get; set; } = OrderStatus.Pending;


        // رقم تتبع الطلب (اختياري لكنه مهم للتتبع)
        public string TrackingCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        // علاقة الطلب بالعناصر
        public List<OrderItem> Items { get; set; } = new();
    }


}
