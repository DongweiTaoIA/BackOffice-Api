using BackOffice.Domain.Models;

namespace BackOffice.Application.Services.Intents;

/// <summary>
/// Carries the user's raw message and any entities extracted from it.
/// Extend with additional fields (e.g. ContractNumber, ClaimNumber) as new
/// intent handlers are added.
/// </summary>
public class ChatContext
{
    public required string Message { get; init; }

    /// <summary>Lower-cased convenience copy of <see cref="Message"/> for keyword matching.</summary>
    public string LowerMessage => Message.ToLowerInvariant();

    public string? DealerCode { get; init; }
    public string? Product { get; init; }

    /// <summary>Specific program code (e.g., "AU220") if the user referenced one.</summary>
    public string? ProgramCode { get; init; }
}

/// <summary>The structured response produced by an intent handler.</summary>
public class ChatResponse
{
    public string Content { get; set; } = string.Empty;
    public EligibilityResult? EligibilityResult { get; set; }
    public List<SuggestedAction>? Suggestions { get; set; }
}

/// <summary>
/// Handles a single category of user intent. Handlers are scored against each
/// incoming message; the highest-scoring handler processes the message.
///
/// To add a new capability: implement this interface in a new handler, then
/// register it in DI (Program.cs). No changes to <c>ChatService</c> are required.
/// </summary>
public interface IChatIntentHandler
{
    /// <summary>
    /// Returns a confidence score for handling the given context.
    /// Return 0 when the handler cannot or should not handle the message.
    /// The handler with the highest positive score is selected.
    /// </summary>
    int Score(ChatContext context);

    /// <summary>Produces a response for the given context.</summary>
    Task<ChatResponse> HandleAsync(ChatContext context);
}
