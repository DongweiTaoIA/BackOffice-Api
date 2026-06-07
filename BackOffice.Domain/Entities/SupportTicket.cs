namespace BackOffice.Domain.Entities;

public class SupportTicket
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "bug" | "feature" | "help"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "New"; // New | InProgress | Resolved | Closed
    public List<SupportTicketComment> Comments { get; set; } = [];
    public List<SupportTicketAttachment> Attachments { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Bug-specific
    public string? Severity { get; set; } // Critical | High | Medium | Low
    public string? StepsToReproduce { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }

    // Feature-specific
    public string? Priority { get; set; } // Must-have | Nice-to-have
    public string? UseCase { get; set; }

    // Help-specific
    public string? Urgency { get; set; } // Blocking | Non-blocking
    public string? RelatedModule { get; set; }

    // Shared
    public string? VideoLink { get; set; }
}

public class SupportTicketComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public List<InlineImage> InlineImages { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InlineImage
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class SupportTicketAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}
