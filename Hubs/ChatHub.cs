using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LampStoreProjects.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> _connections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _connections[Context.ConnectionId] = userId;
                
                // Thông báo user online
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                await Clients.All.SendAsync("UserOnline", userId);
                
                Console.WriteLine($"User {userId} connected with connection {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var userId))
            {
                _connections.Remove(Context.ConnectionId);
                
                // Thông báo user offline
                await Clients.All.SendAsync("UserOffline", userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                Console.WriteLine($"User {userId} disconnected");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Join một chat room cụ thể
        public async Task JoinChat(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            Console.WriteLine($"User joined chat {chatId}");
        }

        // Leave chat room
        public async Task LeaveChat(Guid chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            Console.WriteLine($"User left chat {chatId}");
        }

        // Gửi tin nhắn
        public async Task SendMessage(Guid chatId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return;

            // Gửi tin nhắn tới tất cả members trong chat room
            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", new
            {
                ChatId = chatId,
                SenderId = userId,
                SenderName = userName,
                Content = message,
                Timestamp = DateTime.UtcNow,
                Type = "Text"
            });

            Console.WriteLine($"Message sent from {userName} in chat {chatId}: {message}");
        }

        // Thông báo user đang typing
        public async Task UserTyping(Guid chatId, bool isTyping)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return;

            await Clients.OthersInGroup($"chat_{chatId}").SendAsync("UserTyping", new
            {
                ChatId = chatId,
                UserId = userId,
                UserName = userName,
                IsTyping = isTyping
            });
        }

        // Mark message as read
        public async Task MarkAsRead(Guid chatId, Guid messageId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return;

            await Clients.Group($"chat_{chatId}").SendAsync("MessageRead", new
            {
                ChatId = chatId,
                MessageId = messageId,
                ReadBy = userId,
                ReadAt = DateTime.UtcNow
            });
        }

        // Get online users
        public static List<string> GetOnlineUsers()
        {
            return _connections.Values.Distinct().ToList();
        }

        // Check if user is online
        public static bool IsUserOnline(string userId)
        {
            return _connections.ContainsValue(userId);
        }
    }
} 