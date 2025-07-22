using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LampStoreProjects.Repositories.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Chat operations
        public async Task<LampStoreProjects.Data.Chat> CreateChatAsync(string userId, string subject, ChatPriority priority = ChatPriority.Normal)
        {
            var chat = new LampStoreProjects.Data.Chat
            {
                UserId = userId,
                Subject = subject,
                Priority = priority,
                Status = ChatStatus.Open,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Chats!.Add(chat);
            await _context.SaveChangesAsync();

            return await GetChatByIdAsync(chat.Id) ?? chat;
        }

        public async Task<LampStoreProjects.Data.Chat?> GetChatByIdAsync(Guid chatId)
        {
            return await _context.Chats!
                .Include(c => c.User)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<IEnumerable<LampStoreProjects.Data.Chat>> GetChatsByUserIdAsync(string userId)
        {
            return await _context.Chats!
                .Include(c => c.User)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LampStoreProjects.Data.Chat>> GetAllChatsAsync()
        {
            return await _context.Chats!
                .Include(c => c.User)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LampStoreProjects.Data.Chat>> GetChatsByStatusAsync(ChatStatus status)
        {
            return await _context.Chats!
                .Include(c => c.User)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LampStoreProjects.Data.Chat>> GetUnassignedChatsAsync()
        {
            return await _context.Chats!
                .Include(c => c.User)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
                .Where(c => c.AssignedAdminId == null && c.Status == ChatStatus.Open)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateChatStatusAsync(Guid chatId, ChatStatus status)
        {
            var chat = await _context.Chats!.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null) return false;

            chat.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignChatToAdminAsync(Guid chatId, string adminId)
        {
            var chat = await _context.Chats!.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null) return false;

            chat.AssignedAdminId = adminId;
            chat.Status = ChatStatus.InProgress;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateChatPriorityAsync(Guid chatId, ChatPriority priority)
        {
            var chat = await _context.Chats!.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null) return false;

            chat.Priority = priority;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CloseChatAsync(Guid chatId)
        {
            var chat = await _context.Chats!.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null) return false;

            chat.Status = ChatStatus.Closed;
            await _context.SaveChangesAsync();
            return true;
        }

        // Message operations
        public async Task<Message> SendMessageAsync(Guid chatId, string senderId, string content, MessageType type = MessageType.Text)
        {
            var message = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Content = content,
                Type = type,
                IsRead = false
            };

            _context.Messages!.Add(message);

            // Update last message time in chat
            var chat = await _context.Chats!.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat != null)
            {
                chat.LastMessageAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await _context.Messages!
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .FirstAsync(m => m.Id == message.Id);
        }

        public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId)
        {
            return await _context.Messages!
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId)
        {
            return await _context.Messages!
                .Include(m => m.Chat)
                .Include(m => m.Sender)
                .Where(m => m.Chat.UserId == userId && !m.IsRead && m.SenderId != userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkMessageAsReadAsync(Guid messageId, string userId)
        {
            var message = await _context.Messages!
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId == userId) 
                return false;

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllChatMessagesAsReadAsync(Guid chatId, string userId)
        {
            var messages = await _context.Messages!
                .Where(m => m.ChatId == chatId && !m.IsRead && m.SenderId != userId)
                .ToListAsync();

            if (!messages.Any()) return true;

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadMessageCountAsync(string userId)
        {
            return await _context.Messages!
                .Include(m => m.Chat)
                .CountAsync(m => m.Chat.UserId == userId && !m.IsRead && m.SenderId != userId);
        }

        // Statistics
        public async Task<int> GetTotalChatsCountAsync()
        {
            return await _context.Chats!.CountAsync();
        }

        public async Task<int> GetOpenChatsCountAsync()
        {
            return await _context.Chats!
                .CountAsync(c => c.Status == ChatStatus.Open || c.Status == ChatStatus.InProgress);
        }

        public async Task<int> GetChatsByUserCountAsync(string userId)
        {
            return await _context.Chats!.CountAsync(c => c.UserId == userId);
        }

        public async Task<Dictionary<ChatStatus, int>> GetChatStatisticsAsync()
        {
            var stats = await _context.Chats!
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            // Ensure all statuses are included with 0 count if not present
            foreach (var status in Enum.GetValues<ChatStatus>())
            {
                if (!stats.ContainsKey(status))
                    stats[status] = 0;
            }

            return stats;
        }
    }
} 