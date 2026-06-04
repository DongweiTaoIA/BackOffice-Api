using BackOffice.Domain.Models;

namespace BackOffice.Application.Services.Intents;

/// <summary>
/// Always-applicable handler used when no specialized handler matches.
/// Returns a capability overview. Scores 1 so it only wins when every other
/// handler returns 0.
/// </summary>
public class FallbackIntentHandler : IChatIntentHandler
{
    public int Score(ChatContext context) => 1;

    public Task<ChatResponse> HandleAsync(ChatContext context) =>
        Task.FromResult(new ChatResponse
        {
            Content = "I'm Team PnC, your BackOffice assistant. I can help with dealer eligibility checks. Try asking:\n\n• \"Can BC006642 sell DW?\"\n• \"Is ON008800 eligible for Extended Warranty?\"\n• \"Check AB005500 for GAP\"",
            Suggestions =
            [
                new() { Label = "Can BC006642 sell DW?", Action = "Can BC006642 sell DW?" },
                new() { Label = "Can ON008800 sell GAP?", Action = "Can ON008800 sell GAP?" },
                new() { Label = "Check AB005500 for EW", Action = "Can AB005500 sell EW?" },
            ]
        });
}
