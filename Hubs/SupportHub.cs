using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Models;
using Microsoft.AspNetCore.SignalR;

namespace Keswa_Project.Hubs
{
    public class SupportHub : Hub
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        // نخزن اسم المستخدم بناءً على ConnectionId
        private static readonly Dictionary<string, string> ConnectedUsers = new();

        public SupportHub(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        // العميل أول ما يتصل يستدعي دي ويدخل اسمه
        public async Task RegisterUser(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                ConnectedUsers[Context.ConnectionId] = userName;

                // نخلي كل مستخدم له group باسمه
                await Groups.AddToGroupAsync(Context.ConnectionId, userName);
                Console.WriteLine($"User {userName} registered and joined group {userName}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var userName))
            {
                ConnectedUsers.Remove(Context.ConnectionId);
                Console.WriteLine($"User {userName} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string toUser, string message)
        {
            if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var fromUser))
                throw new HubException("User is not registered");

            var chatMessage = new ChatMessage
            {
                FromUser = fromUser,
                ToUser = toUser,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _chatMessageRepository.CreateAsync(chatMessage);
            await _chatMessageRepository.CommitAsync();

            // ابعت للـ toUser
            await Clients.Group(toUser).SendAsync("ReceiveMessage", fromUser, message, toUser);

            // ابعت للـ fromUser كمان (علشان يشوف رسالته)
            if (fromUser != toUser)
            {
                await Clients.Group(fromUser).SendAsync("ReceiveMessage", fromUser, message, toUser);
            }
        }
    }
}
