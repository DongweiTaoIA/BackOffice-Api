using BackOffice.Domain.Entities;
using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using BackOffice.Infrastructure.BackOfficeAdminData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class ChatStore : IChatStore
{
    private readonly BackOfficeAdminDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatStore(BackOfficeAdminDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetUserId() =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("oid")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;

    public async Task<ChatDto> CreateChatAsync(string title)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Title = title,
            UserId = GetUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();

        return ToDto(chat);
    }

    public async Task<ChatDto?> GetChatAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return null;

        var chat = await _db.Chats
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == guid);

        return chat is null ? null : ToDto(chat);
    }

    public async Task<List<ChatDto>> GetAllChatsAsync()
    {
        var chats = await _db.Chats
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        return chats.Select(c => ToDto(c)).ToList();
    }

    public async Task AddMessageAsync(string chatId, MessageDto message)
    {
        if (!Guid.TryParse(chatId, out var guid)) return;

        var chat = await _db.Chats.FindAsync(guid);
        if (chat is null) return;

        var chatHistory = new ChatHistory
        {
            Id = Guid.NewGuid(),
            ChatId = guid,
            UserId = GetUserId(),
            Role = message.Role,
            Content = message.Content,
            CreatedAt = DateTime.UtcNow
        };

        _db.ChatHistories.Add(chatHistory);
        chat.UpdatedAt = DateTime.UtcNow;

        // Update title from first user message
        if (message.Role == "user" && !await _db.ChatHistories.AnyAsync(m => m.ChatId == guid))
        {
            chat.Title = message.Content.Length > 50
                ? message.Content[..50] + "..."
                : message.Content;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteChatAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return false;

        var chat = await _db.Chats.FindAsync(guid);
        if (chat is null) return false;

        _db.Chats.Remove(chat);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ChatDto ToDto(Chat c) => new()
    {
        Id = c.Id.ToString(),
        Title = c.Title,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Messages = c.Messages.Select(m => new MessageDto
        {
            Id = m.Id.ToString(),
            ChatId = m.ChatId.ToString(),
            Role = m.Role,
            Content = m.Content,
            Timestamp = m.CreatedAt
        }).ToList()
    };
}
