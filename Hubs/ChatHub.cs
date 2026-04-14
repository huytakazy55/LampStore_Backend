using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using System.Security.Claims;
using LampStoreProjects.Repositories.Chat;
using LampStoreProjects.Data;

namespace LampStoreProjects.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // Bug fix: dùng ConcurrentDictionary thay Dictionary để thread-safe
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        private readonly IChatRepository _chatRepository;
        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _connections[Context.ConnectionId] = userId;
                
                // Thông báo user online
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                await Clients.All.SendAsync("UserOnline", userId);
                
                // Tự động join group "admins" nếu là admin
                if (!string.IsNullOrEmpty(userRole) && userRole == "Administrator")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
                    Console.WriteLine($"🎯 Admin {userName} ({userId}) automatically joined admins group with connection {Context.ConnectionId}");
                }
                
                Console.WriteLine($"User {userName} ({userId}) with role {userRole} connected with connection {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryRemove(Context.ConnectionId, out var userId))
            {
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

        // Cho phép client join group 'admins' để nhận thông báo toàn hệ thống
        // Bug fix: kiểm tra role trước khi cho phép join group admins (security)
        public async Task JoinAdminsGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (userRole != "Administrator")
            {
                Console.WriteLine($"⛔ Unauthorized attempt to join admins group by user {userName} ({userId}) with role '{userRole}'");
                throw new HubException("Bạn không có quyền join group admins.");
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            Console.WriteLine($"🎯 User {userName} ({userId}) with role {userRole} joined admins group via connection {Context.ConnectionId}");
        }

        public async Task LeaveAdminsGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
            Console.WriteLine($"🚪 User {userName} ({userId}) left admins group via connection {Context.ConnectionId}");
        }

        // Gửi tin nhắn
        public async Task SendMessage(Guid chatId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            
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

            // Nếu là user gửi (không phải admin), gửi notification tới group 'admins'
            // Dùng event "AdminChatNotification" khác với "ReceiveMessage" để tránh admin nhận tin nhắn 2 lần
            if (string.IsNullOrEmpty(userRole) || userRole != "Administrator")
            {
                try
                {
                    var chat = await _chatRepository.GetChatByIdAsync(chatId);
                    var adminNotification = new
                    {
                        ChatId = chatId,
                        SenderId = userId,
                        SenderName = userName,
                        Content = message,
                        Timestamp = DateTime.UtcNow,
                        Type = "Text",
                        ChatSubject = chat?.Subject,
                        UserName = chat?.User?.UserName,
                        Priority = chat?.Priority
                    };
                    
                    // Gửi event AdminChatNotification (KHÁC ReceiveMessage) tới admins group
                    // Admin nhận ReceiveMessage từ chat room (hiển thị trong chat window)
                    // Admin nhận AdminChatNotification từ admins group (hiển thị notification popup)
                    // => Không bao giờ duplicate
                    await Clients.Group("admins").SendAsync("AdminChatNotification", adminNotification);
                    Console.WriteLine($"📢 Sent AdminChatNotification to admins group from user {userName} in chat {chatId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error sending notification to admins group: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"👨‍💼 Message from admin {userName}, not sending to admins group");
            }

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
            // ConcurrentDictionary không có ContainsValue, dùng LINQ thay thế
            return _connections.Values.Contains(userId);
        }
    }
} 