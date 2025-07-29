using System.ComponentModel.DataAnnotations;

namespace Keswa_Entities.Dtos
{
    public class RegisterDto
    {
        [Length(1,100)]
        public string UserName { get; set; }

        [Length(1,100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        [DataType(DataType.Password)]

        public string ConfirmPassword { get; set; }
    }
}
