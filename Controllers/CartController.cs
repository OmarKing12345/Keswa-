using Kesawa_Data_Access.Data;
using Keswa_Entities.Dtos;
using Keswa_Entities.Models;
using Keswa_Entities.Models.Emum;
using Keswa_Untilities.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stripe.Checkout;
using Order = Keswa_Entities.Models.Order;


namespace Keswa_Project.Controllers
{
    [ApiController]
    [Route("api/cart")]

    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IStringLocalizer<CartController> _localizer;

        public CartController(ICartService cartService, ApplicationDbContext dbContext)
        {
            _cartService = cartService;
            _dbContext = dbContext;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromQuery] string userId, [FromBody] CartItemDto item)
        {
            try
            {
                await _cartService.AddToCartAsync(userId, item.ProductId, item.Quantity);
                return Ok("Product added to cart.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        [HttpGet("cart")]
        public async Task<IActionResult> GetCart([FromQuery] string userId)
        {
            try
            {
                var cart = await _cartService.GetCartWithDetailsAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartItemDto dto)
        {

            try
            {
                if (dto.Quantity <= 0)
                    return BadRequest(_localizer["Quantity"]);
                    return BadRequest("Quantity must be greater than 0");

                var updatedCart = await _cartService.UpdateCartItemAsync(dto.UserId, dto.ProductId, dto.Quantity);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart([FromQuery] string userId, [FromQuery] int productId)
        {
            try
            {
                var updatedCart = await _cartService.RemoveFromCartAsync(userId, productId);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




                var domain = "https://localhost:7061";
                var Successdomain = "http://127.0.0.1:5500/FrontendProject-20250730T213241Z-1-001/FrontendProject";




        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);


                var cart = await _cartService.GetCartAsync(userId);


                if (cartItems == null || cartItems.Count == 0)
                    return BadRequest("Cart is empty or already processed.");

                var totalAmount = cart.Sum(item => item.Price * item.Quantity);


                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    }).ToList()
                };


                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();


                await _cartService.ClearCartAsync(userId);

            }
            catch (Exception ex)
            {
            }
        }
        {
            try
            {

                var cart = await _cartService.GetCartAsync(userId);



                {
                    {
                        {
                            {
    {
    }

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
            }
        }

    }

}
