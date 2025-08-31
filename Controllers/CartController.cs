using Kesawa_Data_Access.Data;
using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Dtos;
using Keswa_Entities.Models;
using Keswa_Entities.Models.Emum;
using Keswa_Untilities.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IProductRepository _productRepository;

        public CartController(ICartService cartService, ApplicationDbContext dbContext, IProductRepository productRepository)
        {
            _cartService = cartService;
            _dbContext = dbContext;
            _productRepository = productRepository;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromQuery] string userId, [FromBody] CartItemDto item)
        {
            try
            {
                await _cartService.AddToCartAsync(userId, item.ProductId, item.Quantity);
                var currectProduct = await _productRepository.GetOneAsync(x => x.Id == item.ProductId);
                if (currectProduct!=null)
                {
                    currectProduct.Count -= item.Quantity;
                    await _productRepository.CommitAsync();
                }
                
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
                    return BadRequest("Quantity must be greater than 0");

                var updatedCart = await _cartService.UpdateCartItemAsync(dto.UserId, dto.ProductId, dto.Quantity);
                var currentProduct = await _productRepository.GetOneAsync(x => x.Id == dto.ProductId);
                if (currentProduct != null)
                {
                    currentProduct.Count -= dto.Quantity;
                    await _productRepository.CommitAsync();
                }
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart([FromQuery] string userId, [FromQuery] int productId, [FromQuery]int Quantity) 
        {
            try
            {
                var updatedCart = await _cartService.RemoveFromCartAsync(userId, productId);
                var currentProduct = await _productRepository.GetOneAsync(x => x.Id == productId);
                if (currentProduct!=null)
                {
                    currentProduct.Count += Quantity;
                    await _productRepository.CommitAsync();
                }
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateStripeSession([FromBody] CheckoutRequest request)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(request.UserId);

                if (cart.Count == 0)
                    return BadRequest("Cart is empty.");

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",

                    // هنا التعديلات
                    SuccessUrl = "http://127.0.0.1:5500/success.html?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = "http://127.0.0.1:5500/cancel.html",

                    LineItems = cart.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(item.Price * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = string.IsNullOrWhiteSpace(item.Name) ? $"Product {item.ProductId}" : item.Name
                            }
                        },
                        Quantity = item.Quantity
                    }).ToList(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", request.UserId }
                    }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                return BadRequest($"Stripe session creation failed: {ex.Message}");
            }
        }

        [HttpGet("order-success")]
        public async Task<IActionResult> OrderSuccess([FromQuery] string session_id)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);

                var userId = session.Metadata["userId"];

                var cartItems = await _cartService.GetCartAsync(userId);

                if (cartItems == null || cartItems.Count == 0)
                    return BadRequest("Cart is empty or already processed.");

                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = cartItems.Sum(item => item.Price * item.Quantity),
                    Status = OrderStatus.Paid,
                    StripeSessionId = session_id,
                    Items = cartItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    }).ToList()
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                await _cartService.ClearCartAsync(userId);

                return Ok($"Order succeed");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to process order: " + ex.Message);
            }
        }

        [HttpPost("save-order")]
        public async Task<IActionResult> SaveOrder([FromQuery] string sessionId)
        {
            try
            {
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(sessionId);

                var lineItemService = new Stripe.Checkout.SessionLineItemService();
                var lineItems = await lineItemService.ListAsync(sessionId);

                var userId = session.Metadata["userId"];
                if (string.IsNullOrEmpty(userId)) return BadRequest("User ID is missing.");

                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Paid,
                    StripeSessionId = sessionId,
                    TotalAmount = (decimal)(session.AmountTotal / 100m),
                    TrackingCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Items = new List<OrderItem>()
                };

                foreach (var item in lineItems)
                {
                    var productIdStr = item.Description;
                    if (!int.TryParse(productIdStr, out int productId))
                        continue;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = productId,
                        Quantity = (int)(item.Quantity ?? 1),
                        UnitPrice = (decimal)(item.AmountSubtotal / 100m / (item.Quantity ?? 1))
                    });
                }

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
                await _cartService.ClearCartAsync(userId);

                return Ok(new
                {
                    success = true,
                    order = new
                    {
                        order.Id,
                        order.TrackingCode,
                        order.TotalAmount,
                        order.CreatedAt,
                        Items = order.Items.Select(i => new
                        {
                            i.ProductId,
                            i.Quantity,
                            i.UnitPrice,
                            SubTotal = i.SubTotal
                        })
                    }
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save order: {ex.Message}");
                return StatusCode(500, "Error saving order.");
            }
        }
    }
}
