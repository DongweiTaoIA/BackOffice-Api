namespace BackOffice.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ChatHistory> Messages { get; set; } = [];
}
