using Kesawa_Data_Access.Data;
using Keswa_Entities.Dtos.Request;
using Keswa_Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Keswa_Project.Controllers.Custmor
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<ProfileController> _localizer;


        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IStringLocalizer<ProfileController> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        [HttpGet("CurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                userId = user.Id,
                username = user.UserName,
                email = user.Email,
                userPhoneNumber = user.PhoneNumber,
                userAddress = user.Address,
                userAge = user.Age,
                userImage = $"{Request.Scheme}://{Request.Host}{user.Image}"
            });
        }

        [HttpPut("UpdateCurrentUser")]
        public async Task<IActionResult> UpdateCurrentUser([FromForm] UpdateCurrentUserRequest updateCurrentUserRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            try
            {
                user.Email = updateCurrentUserRequest.Email ?? user.Email;
                user.UserName = updateCurrentUserRequest.UserName ?? user.UserName;
                user.Address = updateCurrentUserRequest.Address ?? user.Address;
                user.PhoneNumber = updateCurrentUserRequest.PhoneNumber ?? user.PhoneNumber;
                user.Age = updateCurrentUserRequest.Age ?? user.Age;

                if (updateCurrentUserRequest.ImageFile != null && updateCurrentUserRequest.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    if (!string.IsNullOrEmpty(user.Image))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(updateCurrentUserRequest.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateCurrentUserRequest.ImageFile.CopyToAsync(fileStream);
                    }

                    user.Image = "/Images/" + uniqueFileName;
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return BadRequest(updateResult.Errors.Select(e => e.Description));

                if (!string.IsNullOrWhiteSpace(updateCurrentUserRequest.NewPassword) &&
                    !string.IsNullOrWhiteSpace(updateCurrentUserRequest.CurrentPassword))
                {
                    var passwordChangeResult = await _userManager.ChangePasswordAsync(
                        user,
                        updateCurrentUserRequest.CurrentPassword,
                        updateCurrentUserRequest.NewPassword
                    );

                    if (!passwordChangeResult.Succeeded)
                        return BadRequest(passwordChangeResult.Errors.Select(e => e.Description));
                }

                return Ok(_localizer["User updated successfully"]);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
