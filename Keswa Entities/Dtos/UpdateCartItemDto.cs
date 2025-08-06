namespace Keswa_Entities.Dtos
{
    public class UpdateCartItemDto
    {
        public string UserId { get; set; } = null!;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
