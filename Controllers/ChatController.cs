using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using LampStoreProjects.Data;
using LampStoreProjects.Repositories.Chat;
using LampStoreProjects.Hubs;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatRepository chatRepository, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger)
        {
            _chatRepository = chatRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        // Tạo chat mới (User tạo yêu cầu hỗ trợ)
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            try
            {
                _logger.LogInformation("Create chat request received: {@Request}", request);
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("User ID: {UserId}", userId);
                
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (string.IsNullOrEmpty(request.Subject))
                {
                    _logger.LogWarning("Subject is empty");
                    return BadRequest("Subject is required");
                }

                var chat = await _chatRepository.CreateChatAsync(userId, request.Subject, request.Priority);

                // Thông báo cho admins về chat mới
                await _hubContext.Clients.Group("admins").SendAsync("NewChatCreated", new
                {
                    ChatId = chat.Id,
                    UserId = userId,
                    Subject = request.Subject,
                    Priority = request.Priority,
                    CreatedAt = chat.CreatedAt
                });

                return Ok(chat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat");
                return StatusCode(500, "Có lỗi xảy ra khi tạo chat");
            }
        }

        // Lấy danh sách chat của user
        [HttpGet("my-chats")]
        public async Task<IActionResult> GetMyChats()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var chats = await _chatRepository.GetChatsByUserIdAsync(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user chats");
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Lấy tất cả chat (Admin only)
        [HttpGet("all")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllChats()
        {
            try
            {
                var chats = await _chatRepository.GetAllChatsAsync();
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all chats");
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Lấy chat theo ID
        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChat(Guid chatId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null)
                    return NotFound();

                // Kiểm tra quyền truy cập
                if (userRole != "Administrator" && chat.UserId != userId)
                    return Forbid();

                // Mark messages as read
                await _chatRepository.MarkAllChatMessagesAsReadAsync(chatId, userId);

                return Ok(chat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat {ChatId}", chatId);
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Gửi tin nhắn
        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageRequest request)
        {
            try
            {
                _logger.LogInformation("Send message request - ChatId: {ChatId}, Request: {@Request}", chatId, request);
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                _logger.LogInformation("User info - UserId: {UserId}, UserName: {UserName}, Role: {Role}", userId, userName, userRole);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (string.IsNullOrEmpty(request.Content))
                {
                    _logger.LogWarning("Message content is empty");
                    return BadRequest("Message content is required");
                }

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null)
                    return NotFound();

                // Kiểm tra quyền gửi tin nhắn
                if (userRole != "Administrator" && chat.UserId != userId)
                    return Forbid();

                var message = await _chatRepository.SendMessageAsync(chatId, userId, request.Content, request.Type);

                // Gửi tin nhắn realtime tới room chat
                await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", new
                {
                    MessageId = message.Id,
                    ChatId = chatId,
                    SenderId = userId,
                    SenderName = userName,
                    Content = request.Content,
                    Type = request.Type.ToString(),
                    Timestamp = message.CreatedAt,
                    IsRead = false
                });

                // Nếu người gửi KHÔNG phải admin, gửi thêm thông báo tới group 'admins'
                if (string.IsNullOrEmpty(userRole) || userRole != "Administrator")
                {
                    try
                    {
                        // Lấy thông tin chat để gửi kèm cho admin
                        var fullChat = await _chatRepository.GetChatByIdAsync(chatId);
                        var adminNotification = new
                        {
                            ChatId = chatId,
                            SenderId = userId,
                            SenderName = userName,
                            Content = request.Content,
                            Timestamp = message.CreatedAt,
                            Type = request.Type.ToString(),
                            ChatSubject = fullChat?.Subject,
                            UserName = fullChat?.User?.UserName,
                            Priority = fullChat?.Priority
                        };

                        await _hubContext.Clients.Group("admins").SendAsync("AdminChatNotification", adminNotification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error notifying admins for message in chat {ChatId}", chatId);
                    }
                }

                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
                return StatusCode(500, "Có lỗi xảy ra khi gửi tin nhắn");
            }
        }

        // Lấy tin nhắn của chat
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(Guid chatId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null)
                    return NotFound();

                // Kiểm tra quyền truy cập
                if (userRole != "Administrator" && chat.UserId != userId)
                    return Forbid();

                var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId);
                
                // Debug logging
                _logger.LogInformation("GetMessages for chat {ChatId}: Found {Count} messages", chatId, messages.Count());
                foreach (var msg in messages)
                {
                    _logger.LogInformation("Message {Id}: SenderId='{SenderId}', Content='{Content}', Sender='{SenderName}'", 
                        msg.Id, msg.SenderId, msg.Content?.Substring(0, Math.Min(msg.Content.Length, 50)), msg.Sender?.UserName);
                }
                
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for chat {ChatId}", chatId);
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Assign chat cho admin (Admin only)
        [HttpPost("{chatId}/assign")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignChat(Guid chatId, [FromBody] AssignChatRequest request)
        {
            try
            {
                var success = await _chatRepository.AssignChatToAdminAsync(chatId, request.AdminId);
                if (!success)
                    return NotFound();

                // Thông báo realtime
                await _hubContext.Clients.All.SendAsync("ChatAssigned", new
                {
                    ChatId = chatId,
                    AdminId = request.AdminId
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning chat {ChatId}", chatId);
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Cập nhật status chat (Admin only)
        [HttpPut("{chatId}/status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateChatStatus(Guid chatId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var success = await _chatRepository.UpdateChatStatusAsync(chatId, request.Status);
                if (!success)
                    return NotFound();

                // Thông báo realtime
                await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("ChatStatusUpdated", new
                {
                    ChatId = chatId,
                    Status = request.Status.ToString()
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chat status {ChatId}", chatId);
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Đóng chat
        [HttpPost("{chatId}/close")]
        public async Task<IActionResult> CloseChat(Guid chatId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null)
                    return NotFound();

                // Chỉ admin hoặc owner có thể đóng chat
                if (userRole != "Administrator" && chat.UserId != userId)
                    return Forbid();

                var success = await _chatRepository.CloseChatAsync(chatId);
                if (!success)
                    return BadRequest();

                // Thông báo realtime
                await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("ChatClosed", new
                {
                    ChatId = chatId,
                    ClosedBy = userId
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing chat {ChatId}", chatId);
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Lấy số tin nhắn chưa đọc
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var count = await _chatRepository.GetUnreadMessageCountAsync(userId);
                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        // Thống kê chat (Admin only)
        [HttpGet("statistics")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _chatRepository.GetChatStatisticsAsync();
                var totalChats = await _chatRepository.GetTotalChatsCountAsync();
                var openChats = await _chatRepository.GetOpenChatsCountAsync();

                return Ok(new
                {
                    TotalChats = totalChats,
                    OpenChats = openChats,
                    StatusBreakdown = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat statistics");
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }
    }

    // ── Guest Chat Endpoints (no auth required) ──
    [ApiController]
    [Route("api/[controller]")]
    public class GuestChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<GuestChatController> _logger;

        public GuestChatController(IChatRepository chatRepository, IHubContext<ChatHub> hubContext, ILogger<GuestChatController> logger)
        {
            _chatRepository = chatRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        private string? GetGuestToken() => Request.Headers["X-Guest-Token"].FirstOrDefault();

        [HttpPost("guest/create")]
        public async Task<IActionResult> CreateGuestChat([FromBody] GuestCreateChatRequest request)
        {
            try
            {
                var guestToken = GetGuestToken();
                if (string.IsNullOrEmpty(guestToken))
                    return BadRequest("X-Guest-Token header is required");

                if (string.IsNullOrEmpty(request.Subject))
                    return BadRequest("Subject is required");

                var guestName = request.GuestName ?? "Khách vãng lai";
                var chat = await _chatRepository.CreateGuestChatAsync(guestToken, guestName, request.Subject, request.Priority);

                // Notify admins
                await _hubContext.Clients.Group("admins").SendAsync("NewChatCreated", new
                {
                    ChatId = chat.Id,
                    GuestToken = guestToken,
                    GuestName = guestName,
                    Subject = request.Subject,
                    Priority = request.Priority,
                    CreatedAt = chat.CreatedAt,
                    IsGuest = true
                });

                return Ok(chat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guest chat");
                return StatusCode(500, "Có lỗi xảy ra khi tạo chat");
            }
        }

        [HttpGet("guest/my-chats")]
        public async Task<IActionResult> GetGuestChats()
        {
            try
            {
                var guestToken = GetGuestToken();
                if (string.IsNullOrEmpty(guestToken))
                    return BadRequest("X-Guest-Token header is required");

                var chats = await _chatRepository.GetChatsByGuestTokenAsync(guestToken);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest chats");
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        [HttpGet("guest/{chatId}/messages")]
        public async Task<IActionResult> GetGuestMessages(Guid chatId)
        {
            try
            {
                var guestToken = GetGuestToken();
                if (string.IsNullOrEmpty(guestToken))
                    return BadRequest("X-Guest-Token header is required");

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null) return NotFound();
                if (chat.GuestToken != guestToken) return Forbid();

                var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest chat messages");
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }

        [HttpPost("guest/{chatId}/messages")]
        public async Task<IActionResult> SendGuestMessage(Guid chatId, [FromBody] SendMessageRequest request)
        {
            try
            {
                var guestToken = GetGuestToken();
                if (string.IsNullOrEmpty(guestToken))
                    return BadRequest("X-Guest-Token header is required");

                if (string.IsNullOrEmpty(request.Content))
                    return BadRequest("Message content is required");

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null) return NotFound();
                if (chat.GuestToken != guestToken) return Forbid();

                // For guest messages, we save them with SenderId = null in the DB
                var message = await _chatRepository.SendMessageAsync(chatId, null, request.Content, request.Type);

                var guestName = chat.GuestName ?? "Khách vãng lai";

                // Send realtime to chat room
                await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", new
                {
                    MessageId = message.Id,
                    ChatId = chatId,
                    SenderId = $"guest_{guestToken.Substring(0, 8)}",
                    SenderName = guestName,
                    Content = request.Content,
                    Type = request.Type.ToString(),
                    Timestamp = message.CreatedAt,
                    IsRead = false,
                    IsGuest = true
                });

                // Notify admins
                try
                {
                    await _hubContext.Clients.Group("admins").SendAsync("AdminChatNotification", new
                    {
                        ChatId = chatId,
                        SenderId = $"guest_{guestToken.Substring(0, 8)}",
                        SenderName = guestName,
                        Content = request.Content,
                        Timestamp = message.CreatedAt,
                        Type = request.Type.ToString(),
                        ChatSubject = chat.Subject,
                        UserName = guestName,
                        Priority = chat.Priority,
                        IsGuest = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying admins for guest message");
                }

                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending guest message");
                return StatusCode(500, "Có lỗi xảy ra khi gửi tin nhắn");
            }
        }
    }

    // DTOs
    public class CreateChatRequest
    {
        public string Subject { get; set; } = string.Empty;
        public ChatPriority Priority { get; set; } = ChatPriority.Normal;
    }

    public class GuestCreateChatRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string? GuestName { get; set; }
        public ChatPriority Priority { get; set; } = ChatPriority.Normal;
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Text;
    }

    public class AssignChatRequest
    {
        public string AdminId { get; set; } = string.Empty;
    }

    public class UpdateStatusRequest
    {
        public ChatStatus Status { get; set; }
    }
} 