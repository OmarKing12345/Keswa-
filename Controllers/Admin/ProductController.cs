using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Keswa_Entities.Dtos.Response;
using Keswa_Entities.Dtos.Request;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Keswa_Project.Keswa_Entities.Dtos.Response;
using Microsoft.AspNetCore.Authorization;

namespace Keswa_Project.Controllers.Admin
{
    [Route("api/Product")]
    [ApiController]
    [Authorize]
     public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _productRepository.GetQuery()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            var productResponses = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Count = p.Count,
                Views = p.Views,
                Status = p.Status,
                BrandId = p.BrandId,
                BrandName = p.Brand?.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                ProductImages = p.ProductImages.Select(img => new ProductImageResponse
                {
                    Image = img.Image
                }).ToList()
            }).ToList();

            return Ok(productResponses);
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var product = await _productRepository.GetQuery()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (product == null)
                return NotFound();

            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Count = product.Count,
                Views = product.Views,
                Status = product.Status,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                ProductImages = product.ProductImages.Select(img => new ProductImageResponse
                {
                    Image = img.Image
                }).ToList()
            };

            return Ok(response);
        }














        // POST: api/Product/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] ProductRequest productRequest)
        {
            var product = productRequest.Adapt<Product>();
            product.ProductImages = new List<ProductImage>();

            // ابدأ بلوك try-catch هنا لحفظ الصور
            if (productRequest.ProductImages != null && productRequest.ProductImages.Any())
            {
                foreach (var image in productRequest.ProductImages)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

                    try
                    {
                        // تأكد إن المجلد موجود، لو مش موجود هيتم إنشاءه
                        var directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await image.CopyToAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        // لو حصل خطأ في حفظ الصورة، اطبع الرسالة ورجع 500
                        Console.WriteLine($"خطأ في حفظ الصورة: {ex.Message}");
                        return StatusCode(500, $"حدث خطأ أثناء رفع الصورة: {ex.Message}");
                    }

                    product.ProductImages.Add(new ProductImage { Image = fileName });
                }
            }

             
                var productcreated= await _productRepository.CreateAsync(product);
                await _productRepository.CommitAsync();

                if (productcreated!=null)
                    return Created($"{Request.Scheme}://{Request.Host}/api/Product/{product.Id}", product.Adapt<ProductResponse>());
                 
                
                 return BadRequest("لم يتم توليد معرف للمنتج بعد الحفظ. هل توجد مشكلة في قاعدة البيانات؟");
               
            
         
        }

















        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] ProductRequest productRequest)
        {
            var productInDb = await _productRepository.GetQuery()
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productInDb == null)
                return NotFound("Product not found.");

            // لو فيه صور جديدة جايه
            if (productRequest.ProductImages != null && productRequest.ProductImages.Any())
            {
                // حذف الصور القديمة
                foreach (var oldImage in productInDb.ProductImages)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", oldImage.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // رفع الصور الجديدة
                var newImages = new List<ProductImage>();
                foreach (var image in productRequest.ProductImages)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    newImages.Add(new ProductImage { Image = fileName });
                }

                // تحديث الصور بالصور الجديدة فقط
                productInDb.ProductImages = newImages;
            }

            // تحديث باقي الخصائص
            productInDb.Name = productRequest.Name;
            productInDb.Description = productRequest.Description;
            productInDb.Status = productRequest.Status;
            productInDb.Price = productRequest.Price;
            productInDb.Count = productRequest.Count;
            productInDb.Views = productRequest.Views;
            productInDb.BrandId = productRequest.BrandId;
            productInDb.CategoryId = productRequest.CategoryId;

            await _productRepository.CommitAsync();

            return NoContent();
        }












        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetQuery()
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                foreach (var img in product.ProductImages)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", img.Image);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _productRepository.Delete(product);
                await _productRepository.CommitAsync();
                return Ok("Deleted successfully");
            }

            return NotFound();
        }
    }
}
