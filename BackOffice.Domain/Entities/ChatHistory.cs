namespace BackOffice.Domain.Entities;

public class ChatHistory
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Chat? Chat { get; set; }
}
