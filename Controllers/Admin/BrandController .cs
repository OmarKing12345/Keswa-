using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Keswa_Entities.Dtos.Response;
using Keswa_Entities.Dtos.Request;
using Mapster;

namespace Keswa_Project.Controllers.Admin
{
    [Route("api/Brand")]
    [ApiController]

    public class BrandController : ControllerBase
    {
        private readonly IBrandRepository _brandRepository;

        public BrandController(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        // GET: api/Brand
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var brands = await _brandRepository.GetAsync();
            return Ok(brands.ToList().Adapt<List<BrandResponse>>());
        }

        // GET: api/Brand/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);
            if (brand is not null)
            {
                return Ok(brand.Adapt<BrandResponse>());
            }
            return NotFound();
        }

        // POST: api/Brand
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandRequest brandRequest)
        {
            if (brandRequest == null)
                return BadRequest("Invalid brand data.");

            var brand = await _brandRepository.CreateAsync(brandRequest.Adapt<Brand>());
            await _brandRepository.CommitAsync();

            if (brand != null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Brand/{brand.Id}", brand.Adapt<BrandResponse>());
            }

            return BadRequest("Could not create brand.");
        }

        // PUT: api/Brand/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] BrandRequest brandRequest)
        {
            if (brandRequest == null)
                return BadRequest();

            var brand = brandRequest.Adapt<Brand>();
            brand.Id = id;

            _brandRepository.Update(brand);
            await _brandRepository.CommitAsync();

            return NoContent();
        }

        // DELETE: api/Brand/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);
            if (brand != null)
            {
                _brandRepository.Delete(brand);
                await _brandRepository.CommitAsync();
                return Ok("Deleted successfully");
            }

            return NotFound();
        }
    }
}
