using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Dtos.Request
{
    public class UpdateCurrentUserRequest
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }

        public string? NewPassword { get; set; }
        public string? CurrentPassword { get; set; }

        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public IFormFile? ImageFile { get; set; }


    }
}
