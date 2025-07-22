using LampStoreProjects.Data;

namespace LampStoreProjects.Repositories.Chat
{
    public interface IChatRepository
    {
        // Chat operations
        Task<LampStoreProjects.Data.Chat> CreateChatAsync(string userId, string subject, ChatPriority priority = ChatPriority.Normal);
        Task<LampStoreProjects.Data.Chat?> GetChatByIdAsync(Guid chatId);
        Task<IEnumerable<LampStoreProjects.Data.Chat>> GetChatsByUserIdAsync(string userId);
        Task<IEnumerable<LampStoreProjects.Data.Chat>> GetAllChatsAsync();
        Task<IEnumerable<LampStoreProjects.Data.Chat>> GetChatsByStatusAsync(ChatStatus status);
        Task<IEnumerable<LampStoreProjects.Data.Chat>> GetUnassignedChatsAsync();
        Task<bool> UpdateChatStatusAsync(Guid chatId, ChatStatus status);
        Task<bool> AssignChatToAdminAsync(Guid chatId, string adminId);
        Task<bool> UpdateChatPriorityAsync(Guid chatId, ChatPriority priority);
        Task<bool> CloseChatAsync(Guid chatId);

        // Message operations
        Task<Message> SendMessageAsync(Guid chatId, string senderId, string content, MessageType type = MessageType.Text);
        Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId);
        Task<bool> MarkMessageAsReadAsync(Guid messageId, string userId);
        Task<bool> MarkAllChatMessagesAsReadAsync(Guid chatId, string userId);
        Task<int> GetUnreadMessageCountAsync(string userId);

        // Statistics
        Task<int> GetTotalChatsCountAsync();
        Task<int> GetOpenChatsCountAsync();
        Task<int> GetChatsByUserCountAsync(string userId);
        Task<Dictionary<ChatStatus, int>> GetChatStatisticsAsync();
    }
} 