namespace Keswa_Entities.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // السعر الفردي وقت الطلب (مش السعر الحالي للمنتج)
        public decimal UnitPrice { get; set; }

        // خاصية مساعدة لحساب المجموع الفرعي (اختياري)
        public decimal SubTotal => UnitPrice * Quantity;
    }


}