using System.ComponentModel.DataAnnotations;

namespace Keswa_Entities.Dtos
{
    public class LoginDto
    {
        [Length(1,100)]
        public string UserNameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
