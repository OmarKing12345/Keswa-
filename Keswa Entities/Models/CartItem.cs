namespace Keswa_Entities.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // تفاصيل إضافية لعرض الكارت
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
    }


}
