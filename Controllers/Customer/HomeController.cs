using AutoMapper;
using Kesawa_Data_Access.Repository;
using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Dtos.Request;
using Keswa_Entities.Dtos.Response;
using Keswa_Entities.Models;
using Keswa_Project.Keswa_Entities.Dtos.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;

namespace Keswa_Project.Controllers.Custmor
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IBrandRepository brandRepository;
        private readonly IMapper _mapper;

        public HomeController(IProductRepository productRepository, IMapper mapper, ICategoryRepository categoryRepository, IBrandRepository brandRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
            this.brandRepository = brandRepository;
            _mapper = mapper;

        }

        [HttpGet("all")]
        public async Task<IActionResult> GetProducts([FromQuery] HomeRequest homeRequest)
        {
            Expression<Func<Product, bool>> expression = p =>
                (homeRequest.MinPrice <= 0 || p.Price >= homeRequest.MinPrice) &&
                (homeRequest.MaxPrice <= 0 || p.Price <= homeRequest.MaxPrice) &&
                (homeRequest.CategoryId <= 0 || p.CategoryId == homeRequest.CategoryId) &&
                (homeRequest.BrandId <= 0 || p.BrandId == homeRequest.BrandId);

            var products = await productRepository.GetAsync(expression: expression,
                includes: [ p => p.Category, p => p.Brand, p => p.ProductImages]);

            if (homeRequest.PageNumber >= 1)
            {
                products = products.Skip((homeRequest.PageNumber - 1) * homeRequest.PageSize).Take(homeRequest.PageSize).ToList();
            }

            var Categories = await categoryRepository.GetAsync();
            var Brands = await brandRepository.GetAsync();

            var productResponse = _mapper.Map<List<ProductResponse>>(products);
            var CategoryResponse = _mapper.Map<List<CategoryHomeResponse>>(Categories);
            var BrandsResponse = _mapper.Map<List<BrandHomeResponse>>(Brands);
            var HomeResponse = new HomeResponse()
            {
                Products = productResponse,
                Categories = CategoryResponse,
                Brands = BrandsResponse

            };
            return Ok(HomeResponse);
        }
    }
}
