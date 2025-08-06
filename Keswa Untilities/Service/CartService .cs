using Kesawa_Data_Access.Data;
using Keswa_Entities.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Keswa_Untilities.Service
{
    public class CartService : ICartService
    {
        private readonly IDatabase _redisDb;
        private readonly string _prefix = "cart:";
        private readonly ApplicationDbContext _dbContext;


        public CartService(IConnectionMultiplexer redis, ApplicationDbContext context)
        {
            _redisDb = redis.GetDatabase();
            _dbContext = context;
        }




        public async Task<List<CartItem>> GetCartAsync(string userId)
        {
            var key = _prefix + userId;
            var cartJson = await _redisDb.StringGetAsync(key);
            return cartJson.IsNullOrEmpty ? new List<CartItem>() :
                JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
        }

        public async Task ClearCartAsync(string userId)
        {
            await _redisDb.KeyDeleteAsync(_prefix + userId);
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var key = _prefix + userId;
            var cartJson = await _redisDb.StringGetAsync(key);

            List<CartItem> cart = cartJson.IsNullOrEmpty
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;

            var existing = cart.FirstOrDefault(x => x.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                var product = await _dbContext.Products.FindAsync(productId);
                if (product == null)
                    throw new Exception("Product not found");

                cart.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Name = product.Name,
                    Price = (decimal)product.Price,
                    Image = product.ImageUrl // عدّل لو اسم الخاصية مختلف
                });
            }

            await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(cart));
        }


        public async Task<List<CartItem>> GetCartWithDetailsAsync(string userId)
        {
            var key = _prefix + userId;
            var cartJson = await _redisDb.StringGetAsync(key);
            var cartItems = cartJson.IsNullOrEmpty
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;

            var productIds = cartItems.Select(x => x.ProductId).ToList();
            var products = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            return cartItems.Select(item => new CartItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Name = products[item.ProductId].Name,
                Price = (decimal)products[item.ProductId].Price,
                Image = products[item.ProductId].ImageUrl
            }).ToList();
        }
        public async Task<List<CartItem>> UpdateCartItemAsync(string userId, int productId, int quantity)
        {
            var key = _prefix + userId;
            var cartJson = await _redisDb.StringGetAsync(key);

            if (cartJson.IsNullOrEmpty)
                throw new Exception("Cart not found");

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
                throw new Exception("Item not found in cart");

            item.Quantity = quantity;
            await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(cart));
            return cart;
        }
        public async Task<List<CartItem>> RemoveFromCartAsync(string userId, int productId)
        {
            var key = _prefix + userId;
            var cartJson = await _redisDb.StringGetAsync(key);

            if (cartJson.IsNullOrEmpty)
                throw new Exception("Cart not found");

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
                throw new Exception("Item not found in cart");

            cart.Remove(item);

            if (cart.Count == 0)
            {
                await _redisDb.KeyDeleteAsync(key); // حذف السلة بالكامل لو فاضية
            }
            else
            {
                await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(cart));
            }

            return cart;
        }


    }

}
