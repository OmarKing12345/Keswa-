using System.ComponentModel.DataAnnotations;

namespace Keswa_Entities.Dtos
{
    public class ResetPasswordDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
        public string ConfirmNewPassword { get; set; }


    }
}
