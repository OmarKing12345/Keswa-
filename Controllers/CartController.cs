using Kesawa_Data_Access.Data;
using Keswa_Entities.Dtos;
using Keswa_Entities.Models;
using Keswa_Entities.Models.Emum;
using Keswa_Untilities.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stripe.Checkout;

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
                return Ok(_localizer["Product added"]);
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

        //[HttpPost("checkout")]
        //public async Task<IActionResult> Checkout([FromQuery] string userId)
        //{
        //    try
        //    {
        //        var cart = await _cartService.GetCartAsync(userId);

        //        if (cart.Count == 0) return BadRequest("Cart is empty.");

        //        // حفظ الطلب في قاعدة البيانات
        //        var order = new Order
        //        {
        //            UserId = userId,
        //            CreatedAt = DateTime.UtcNow,
        //            Items = cart.Select(item => new OrderItem
        //            {
        //                ProductId = item.ProductId,
        //                Quantity = item.Quantity,
        //                Price = item.Price,
        //            }).ToList(),


        //        };

        //        _dbContext.Orders.Add(order);

        //        await _dbContext.SaveChangesAsync();

        //        // امسح الكاش بعد الشراء
        //        await _cartService.ClearCartAsync(userId);

        //        return Ok(new { message = "Order created successfully", orderId = order.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Checkout failed: {ex.Message}");
        //    }
        //}

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {

                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(_localizer["User is not authenticated"]);

                var cart = await _cartService.GetCartAsync(userId);

                if (cart.Count == 0)
                    return BadRequest(_localizer["Cart is empty."]);


                var totalAmount = cart.Sum(item => item.Price * item.Quantity);


                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = totalAmount,
                    TrackingCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Items = cart.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    }).ToList()
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();


                await _cartService.ClearCartAsync(userId);

                return Ok(new
                {
                    message = _localizer["Order created successfully"],
                    orderId = order.Id,
                    trackingCode = order.TrackingCode,
                    total = order.TotalAmount,
                    createdAt = order.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Checkout failed: {ex.Message}");
            }
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateStripeSession()
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(_localizer["User is not authenticated"]);

                var cart = await _cartService.GetCartAsync(userId);

                if (cart.Count == 0)
                    return BadRequest(_localizer["Cart is empty."]);

                var domain = "https://localhost:7061";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",
                    SuccessUrl = domain + "/success?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = domain + "/cancel",
                    LineItems = cart.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(item.Price * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Name ?? $"Product {item.ProductId}"
                            }
                        },
                        Quantity = item.Quantity
                    }).ToList(),
                    Metadata = new Dictionary<string, string>
    {
        { "userId", userId }
    }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                return BadRequest($"{_localizer["Stripe session creation failed"]}: {ex.Message}");
            }
        }

    }

}
