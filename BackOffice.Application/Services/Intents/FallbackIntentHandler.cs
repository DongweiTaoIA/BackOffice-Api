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
            Content = ChatLocalizer.FallbackGreeting(context.Language),
            Suggestions =
            [
                new() { Label = ChatLocalizer.LabelCanSell(context.Language, "BC006642", "DW"), Action = "Can BC006642 sell DW?" },
                new() { Label = ChatLocalizer.LabelCanSell(context.Language, "ON008800", "GAP"), Action = "Can ON008800 sell GAP?" },
                new() { Label = ChatLocalizer.LabelCheckDealerForProduct(context.Language, "AB005500", "EW"), Action = "Can AB005500 sell EW?" },
            ]
        });
}
