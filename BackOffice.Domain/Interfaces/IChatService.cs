using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IChatService
{
    Task<MessageDto> ProcessMessageAsync(string chatId, string userMessage);
}
