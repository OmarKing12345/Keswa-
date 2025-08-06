using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Dtos.Request;
using Keswa_Entities.Dtos.Response;
using Keswa_Entities.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
namespace Keswa_Project.Controllers.Admin

{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IStringLocalizer<CategoryController> _localizer;


        public CategoryController(ICategoryRepository categoryRepository, IStringLocalizer<CategoryController> localizer)
        {
            _categoryRepository = categoryRepository;
            _localizer = localizer;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var categories = await _categoryRepository.GetAsync();
            return Ok(categories.ToList().Adapt<List<CategoryResponse>>());
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Getone([FromRoute]int Id)
        {
            var Category =await _categoryRepository.GetOneAsync(e => e.Id == Id);

            if (Category is not null)
            {
                //TypeAdapterConfig typeAdapterConfig = new();
                //typeAdapterConfig.NewConfig<Category, CategoryResponse>().Map(des => des, src => src.Description);

                return Ok(Category.Adapt<CategoryResponse>() );
            }
            return NotFound();

        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequest categotyRequest)
        {

            var category = await _categoryRepository.CreateAsync(categotyRequest.Adapt<Category>());
            await _categoryRepository.CommitAsync();

            if (category is not null)
            {

                return Created($"{Request.Scheme}://{Request.Host}/api/Category/{category.Id}", category.Adapt<CategoryResponse>());

            }
            return BadRequest();    

        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] CategoryRequest categoryRequest)
        {
            if (categoryRequest is null)
                return BadRequest();

            var category = categoryRequest.Adapt<Category>();
            category.Id = id; // ✅ مهم علشان تحدد ID

            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {
                 _categoryRepository.Delete(category);
                await _categoryRepository.CommitAsync();
                return Ok(_localizer["Deleted successfully"]);
            }

            return NotFound();
        }
 


    }
}
