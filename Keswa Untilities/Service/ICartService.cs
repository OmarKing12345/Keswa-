using Keswa_Entities.Models;

namespace Keswa_Untilities.Service
{
    public interface ICartService
    {
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task<List<CartItem>> GetCartAsync(string userId);
        Task ClearCartAsync(string userId);
        Task<List<CartItem>> GetCartWithDetailsAsync(string userId);
        Task<List<CartItem>> UpdateCartItemAsync(string userId, int productId, int quantity);
        Task<List<CartItem>> RemoveFromCartAsync(string userId, int productId);


    }

}
