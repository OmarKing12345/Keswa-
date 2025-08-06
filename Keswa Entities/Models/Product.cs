using System.ComponentModel.DataAnnotations; // ده موجود عندك

namespace Keswa_Entities.Models
{
    public class Product
    {
        //    [Key] // ده بيعرف EF Core إن ده المفتاح الأساسي (Primary Key) للجدول
        //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // **السطر ده هو الأهم**: بيقول لقاعدة البيانات تولد الـ Id تلقائيًا
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; }
        public Double Price { get; set; }
        public int Count { get; set; }
        public int Views { get; set; } // تأكد إن ده ليه قيمة افتراضية أو بتديها له في الكود (زي ما عملت في الـ Edit)
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; }

        public List<ProductCart> ProductCarts { get; set; }
        public List<ProductOrder> ProductOrders { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<ProductImage> ProductImages { get; set; }
    }
}