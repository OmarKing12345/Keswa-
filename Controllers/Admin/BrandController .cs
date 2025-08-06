using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Dtos.Request;
using Keswa_Entities.Dtos.Response;
using Keswa_Entities.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Keswa_Project.Controllers.Admin
{
    [Route("api/Brand")]
    [ApiController]

    public class BrandController : ControllerBase
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IStringLocalizer<BrandController> _localizer;


        public BrandController(IBrandRepository brandRepository, IStringLocalizer<BrandController> localizer)
        {
            _brandRepository = brandRepository;
            _localizer = localizer;
        }

        // GET: api/Brand
        [Authorize]
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
                return BadRequest(_localizer["Invalid brand data"]);

            var brand = await _brandRepository.CreateAsync(brandRequest.Adapt<Brand>());
            await _brandRepository.CommitAsync();

            if (brand != null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Brand/{brand.Id}", brand.Adapt<BrandResponse>());
            }

            return BadRequest(_localizer["Could not create brand"]);
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
                return Ok(_localizer[""]);
            }

            return NotFound();
        }
    }
}
