using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Untilities.Service
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static ApiResponse<T> Success(T data, string message = null) =>
            new ApiResponse<T> { Succeeded = true, Data = data, Message = message };

        public static ApiResponse<T> Failure(IEnumerable<string> errors) =>
            new ApiResponse<T> { Succeeded = false, Errors = errors };
    }
}
