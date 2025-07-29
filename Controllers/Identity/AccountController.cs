using Keswa_Entities.Dtos;
using Keswa_Entities.Models;
using Keswa_Untilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Keswa_Project.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> signIn;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signIn, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            this.signIn = signIn;
            _roleManager = roleManager;
        }

        [HttpPost("RegisterAccount")]
        public async Task<IActionResult> RegisterAccount(RegisterDto registerVM)
        {
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.SuperAdmin));
                await _roleManager.CreateAsync(new IdentityRole(SD.USer));
                await _roleManager.CreateAsync(new IdentityRole(SD.Company));
            }

            var applicationUser = new ApplicationUser()
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email
            };
            var result = await _userManager.CreateAsync(applicationUser, registerVM.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(applicationUser, SD.USer);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { area = "Identity", applicationUser.Id, token }, Request.Scheme);
                await _emailSender.SendEmailAsync(applicationUser.Email, "Confirmation Email", $"<h1>Confirm Your Account By Click <a href='{confirmationLink}'>Here</a></h1>");
                return Ok("Registered Successfully");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string Id, string token)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return Ok("Email confirmed successfully");
                }
                else
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginVM)
        {
            var applicationUser = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);
            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);
            }

            if (applicationUser != null)
            {
                var result = await _userManager.CheckPasswordAsync(applicationUser, loginVM.Password);
                if (result)
                {
                    // ✅ إعداد claims يدويًا وإضافة ClaimTypes.Name
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, applicationUser.UserName), // مهم لـ SignalR
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Id),
                new Claim("Email", applicationUser.Email ?? "")
            };

                    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties
                    {
                        IsPersistent = true
                    });

                    return Ok("Sign in successfully");
                }
                return BadRequest("Invalid password");
            }
            return BadRequest("User not found");
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordVM)
        {
            var applicationUser = await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOrEmail);
            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOrEmail);
            }
            if (applicationUser is not null)
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                var resetPassword = Url.Action("ResetPassword", "Account", new { area = "Identity", applicationUser.Id, token, forgetPasswordVM.NewPassword, ConfirmPassword = forgetPasswordVM.ConfirmNewPassword }, Request.Scheme);
                await _emailSender.SendEmailAsync(applicationUser.Email, "Reset Password", $"<h1>Reset Password Account By Click <a href='{resetPassword}'>Here</a></h1>");
                return Ok("The email sent successfully");
            }
            return BadRequest();
        }

        [HttpGet("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Password has been reset successfully.");
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }
        [HttpPost("sign out")]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return BadRequest("No user is currently signed in.");
                }
                await signIn.SignOutAsync();
                return Ok("Sign out successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ExternalLogin")]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = signIn.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= "http://127.0.0.1:5500/login.html"; // Default if not provided

            if (remoteError != null)
            {
                return Redirect($"{returnUrl}?error={Uri.EscapeDataString(remoteError)}");
            }

            var info = await signIn.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect($"{returnUrl}?error=Login+info+not+available");
            }

            var signInResult = await signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
            {
                // جلب اسم المستخدم الحالي
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var username = user?.Email?.Split('@')[0] ?? "";
                var separator = returnUrl.Contains("?") ? "&" : "?";
                return Redirect($"{returnUrl}{separator}username={Uri.EscapeDataString(username)}");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email
                    };
                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        return Redirect($"{returnUrl}?error=Error+creating+user");
                    }
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    return Redirect($"{returnUrl}?error=Error+linking+external+login");
                }

                await signIn.SignInAsync(user, isPersistent: false);
                // جلب اسم المستخدم الحالي بعد الإنشاء أو الربط
                var username = user?.UserName ?? "";
                var separator = returnUrl.Contains("?") ? "&" : "?";
                return Redirect($"{returnUrl}{separator}username={Uri.EscapeDataString(username)}");
            }

            return Redirect($"{returnUrl}?error=Email+claim+not+found");
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

            return Ok(new { username = user.UserName, email = user.Email });
        }

        

    }
}