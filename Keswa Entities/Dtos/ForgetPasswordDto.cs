using System.ComponentModel.DataAnnotations;

namespace Keswa_Entities.Dtos
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "UserName or Email is required.")]
        public string UserNameOrEmail { get; set; }
    }
}
