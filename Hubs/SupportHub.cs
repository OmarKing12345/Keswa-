using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Keswa_Project.Hubs
{
    public class SupportHub : Hub
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public SupportHub(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userName);
                Console.WriteLine($"User {userName} joined their group");
            }
            else
            {
                Console.WriteLine("Anonymous user connected");
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string toUser, string message)
        {
            var fromUser = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(fromUser))
                throw new HubException("User is not authenticated");

            var chatMessage = new ChatMessage
            {
                FromUser = fromUser,
                ToUser = toUser,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _chatMessageRepository.CreateAsync(chatMessage);
            await _chatMessageRepository.CommitAsync();

            await Clients.Group(toUser).SendAsync("ReceiveMessage", fromUser, message, toUser);
            if (fromUser != toUser)
            {
                await Clients.Group(fromUser).SendAsync("ReceiveMessage", fromUser, message, toUser);
            }
        }

    }
}
