namespace BackOffice.Domain.Models;

public class ChatDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MessageDto> Messages { get; set; } = [];
}

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public EligibilityResult? EligibilityResult { get; set; }
    public List<SuggestedAction>? Suggestions { get; set; }
}

public class SuggestedAction
{
    public string Label { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Payload { get; set; }
}

public class EligibilityResult
{
    public bool IsEligible { get; set; }
    public string DealerCode { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public List<ProgramInfo> Programs { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}

public class ProgramInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class EligibilityQuery
{
    public string? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProgramId { get; set; }
    public string? ProgramName { get; set; }
}

public class ActivateRequest
{
    public string DealerId { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string? EffectiveDate { get; set; }
}

public class DeactivateRequest
{
    public string DealerId { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
}

public class ProgramActionResult
{
    public bool Success { get; set; }
    public string DealerCode { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class CancellationEligibilityResult
{
    public bool IsEligible { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
    public string RefundType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class MaxMarkupResult
{
    public bool Found { get; set; }
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public decimal MaxMarkup { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ChatId { get; set; }
}

public class CreateChatRequest
{
    public string Title { get; set; } = "New Chat";
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ReportBugRequest
{
    public string Description { get; set; } = string.Empty;
    public string? ChatId { get; set; }
    public string? MessageId { get; set; }
}
