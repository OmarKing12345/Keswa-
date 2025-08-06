using Keswa_Entities.Dtos;
using Keswa_Entities.Models;
using Keswa_Project.Controllers.Admin;
using Keswa_Untilities;
using Keswa_Untilities.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Keswa_Project.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IStringLocalizer<AccountController> _localizer;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IStringLocalizer<AccountController> localizer)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAccount([FromBody] RegisterDto registerVM)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Failure(ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

                await EnsureRolesExist();

                var applicationUser = new ApplicationUser
                {
                    UserName = registerVM.UserName,
                    Email = registerVM.Email
                };

                var result = await _userManager.CreateAsync(applicationUser, registerVM.Password);
                if (!result.Succeeded)
                    return BadRequest(ApiResponse<string>.Failure(result.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(applicationUser, SD.USer);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                var confirmationLink = GenerateEmailConfirmationLink(applicationUser.Id, token);
                await _emailSender.SendEmailAsync(
                    applicationUser.Email,
                    "Confirm Your Email",
                    $"<h1>Please confirm your account by clicking <a href='{confirmationLink}'>here</a></h1>");

                var resultOk = _localizer["Successful Registeration"];
                return Ok(ApiResponse<string>.Success(resultOk.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, ApiResponse<string>.Failure(new[] {_localizer["Registeration Error"].ToString()}));
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginVM)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Failure(ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var user = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail)
                ?? await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);

            if (user == null)
                return BadRequest(ApiResponse<string>.Failure(new[] { _localizer["Invalid credentials"].ToString() }));

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest(ApiResponse<string>.Failure(new[] { _localizer["email confirmation"].ToString() }));

            var result = await _userManager.CheckPasswordAsync(user, loginVM.Password);
            if (!result)
                return BadRequest(ApiResponse<string>.Failure(new[] { _localizer["Invalid credentials"].ToString() }));




            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
             {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("Email", user.Email ?? "")

             };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("eqlkjrljlejrljljghhljflwqlfhuhfuhougoivsldjckklzlkjvlsajkvhjkqevkjeqkjfvukqlv"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var JWToken = new JwtSecurityToken
                (
                 issuer: "https://localhost:7061/",
                 audience: "http://localhost:4200/",
                 claims: claims,
                 expires: DateTime.UtcNow.AddHours(2),
                 signingCredentials: creds
            );


            var tokenString = new JwtSecurityTokenHandler().WriteToken(JWToken);

            return Ok(new { token = tokenString });


        }

        private async Task EnsureRolesExist()
        {
            if (_roleManager.Roles.Any()) return;

            var roles = new[] { SD.Admin, SD.SuperAdmin, SD.USer, SD.Company };
            foreach (var role in roles)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private async Task<List<Claim>> BuildUserClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
                new("Email", user.Email ?? "")
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }

        private async Task SignInUser(List<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal,
                new AuthenticationProperties { IsPersistent = true });
        }

        private string GenerateEmailConfirmationLink(string userId, string token)
        {
            return Url.Action("ConfirmEmail", "Account",
                new { area = "Identity", userId, token },
                Request.Scheme);
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
                await _emailSender.SendEmailAsync(applicationUser.Email, "Reset Password", $"<h1>{_localizer["Reset Password"]}<a href='{resetPassword}'>{_localizer["here"]}</a></h1>");
                return Ok(_localizer["email send"]);
            }
            return BadRequest();
        }

        [HttpGet("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return BadRequest(_localizer["User not found"]);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(_localizer["Password reset"]);
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
                    return BadRequest(_localizer["No user"]);
                }
                await _signInManager.SignOutAsync();
                return Ok(_localizer["Sign out"]);
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
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
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

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect($"{returnUrl}?error=Login+info+not+available");
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
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

                await _signInManager.SignInAsync(user, isPersistent: false);
                // جلب اسم المستخدم الحالي بعد الإنشاء أو الربط
                var username = user?.UserName ?? "";
                var separator = returnUrl.Contains("?") ? "&" : "?";
                return Redirect($"{returnUrl}{separator}username={Uri.EscapeDataString(username)}");
            }

            return Redirect($"{returnUrl}?error=Email+claim+not+found");
        }

    }
}