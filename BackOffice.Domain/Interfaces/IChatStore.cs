using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IChatStore
{
    Task<ChatDto> CreateChatAsync(string title);
    Task<ChatDto?> GetChatAsync(string id);
    Task<List<ChatDto>> GetAllChatsAsync();
    Task AddMessageAsync(string chatId, MessageDto message);
    Task<bool> DeleteChatAsync(string id);
}
