using Kesawa_Data_Access.Data;
using Keswa_Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Keswa_Untilities.Service
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.ProductCarts)
                .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Name == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Name = userId,
                    ProductCarts = new List<ProductCart>()
                };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.ProductCarts.FirstOrDefault(pc => pc.ProductId == productId);
            if (existingItem != null)
            {
                // لو عايز تحسبها بالكوانتيتي، محتاج تضيف Quantity field في ProductCart
                // existingItem.Quantity += quantity; 
            }
            else
            {
                cart.ProductCarts.Add(new ProductCart
                {
                    ProductId = productId,
                    Cart = cart
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<CartItem>> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.ProductCarts)
                .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Name == userId);

            if (cart == null)
                return new List<CartItem>();

            return cart.ProductCarts.Select(pc => new CartItem
            {
                ProductId = pc.ProductId,
                Name = pc.Product?.Name,
                Quantity = 1 // مؤقت لحد ما تضيف Quantity في ProductCart
            }).ToList();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.ProductCarts)
                .FirstOrDefaultAsync(c => c.Name == userId);

            if (cart != null)
            {
                _context.ProductCarts.RemoveRange(cart.ProductCarts);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CartItem>> GetCartWithDetailsAsync(string userId)
        {
            return await GetCartAsync(userId);
        }

        public async Task<List<CartItem>> UpdateCartItemAsync(string userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.ProductCarts)
                .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Name == userId);

            if (cart != null)
            {
                var existingItem = cart.ProductCarts.FirstOrDefault(pc => pc.ProductId == productId);
                if (existingItem != null)
                {
                    // لو فيه Quantity field هتعدلها هنا
                    // existingItem.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
            }

            return await GetCartAsync(userId);
        }

        public async Task<List<CartItem>> RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.ProductCarts)
                .FirstOrDefaultAsync(c => c.Name == userId);

            if (cart != null)
            {
                var existingItem = cart.ProductCarts.FirstOrDefault(pc => pc.ProductId == productId);
                if (existingItem != null)
                {
                    cart.ProductCarts.Remove(existingItem);
                    _context.ProductCarts.Remove(existingItem);
                    await _context.SaveChangesAsync();
                }
            }

            return await GetCartAsync(userId);
        }
    }
}
