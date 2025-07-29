using Kesawa_Data_Access.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Keswa_Project.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessageController : ControllerBase
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public ChatMessageController(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(string user)
        {
            var messages = await _chatMessageRepository.GetAsync(
       m => (m.FromUser == "admin" && m.ToUser == user) || (m.FromUser == user && m.ToUser == "admin")
   );
            messages = messages.OrderBy(m => m.Timestamp).ToList();
            return Ok(messages);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var messages = await _chatMessageRepository.GetAsync(m => m.ToUser == "admin" || m.FromUser == "admin");
            var users = messages
                .Select(m => m.FromUser == "admin" ? m.ToUser : m.FromUser)
                .Where(u => u != "admin")
                .Distinct()
                .ToList();
            return Ok(users);
        }
    }
}
