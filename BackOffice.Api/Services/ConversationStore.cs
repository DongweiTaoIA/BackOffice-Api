using System.Collections.Concurrent;
using BackOffice.Api.Models;

namespace BackOffice.Api.Services;

public interface IConversationStore
{
    ConversationDto CreateConversation(string title);
    ConversationDto? GetConversation(string id);
    List<ConversationDto> GetAllConversations();
    void AddMessage(string conversationId, MessageDto message);
    bool DeleteConversation(string id);
}

public class InMemoryConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<string, ConversationDto> _conversations = new();

    public ConversationDto CreateConversation(string title)
    {
        var conversation = new ConversationDto
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = []
        };

        _conversations[conversation.Id] = conversation;
        return conversation;
    }

    public ConversationDto? GetConversation(string id)
    {
        return _conversations.GetValueOrDefault(id);
    }

    public List<ConversationDto> GetAllConversations()
    {
        return _conversations.Values
            .OrderByDescending(c => c.UpdatedAt)
            .ToList();
    }

    public void AddMessage(string conversationId, MessageDto message)
    {
        if (_conversations.TryGetValue(conversationId, out var conversation))
        {
            conversation.Messages.Add(message);
            conversation.UpdatedAt = DateTime.UtcNow;

            // Update title from first user message
            if (conversation.Messages.Count == 1 && message.Role == "user")
            {
                conversation.Title = message.Content.Length > 50
                    ? message.Content[..50] + "..."
                    : message.Content;
            }
        }
    }

    public bool DeleteConversation(string id)
    {
        return _conversations.TryRemove(id, out _);
    }
}
