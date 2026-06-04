using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;

namespace BackOffice.Application.Services.Intents;

/// <summary>
/// Handles dealer eligibility questions such as "Can BC006642 sell DW?".
/// Migrated from the original ChatService if/else logic.
/// </summary>
public class EligibilityIntentHandler : IChatIntentHandler
{
    private readonly IDealerService _dealerService;
    private readonly IProductRegistry _productRegistry;

    public EligibilityIntentHandler(IDealerService dealerService, IProductRegistry productRegistry)
    {
        _dealerService = dealerService;
        _productRegistry = productRegistry;
    }

    public int Score(ChatContext context)
    {
        var score = 0;

        if (HasEligibilityIntent(context.Message))
            score += 2;

        if (context.DealerCode != null)
            score += 2;

        if (context.Product != null)
            score += 1;

        // Both entities present is a very strong eligibility signal.
        if (context.DealerCode != null && context.Product != null)
            score += 3;

        return score;
    }

    public async Task<ChatResponse> HandleAsync(ChatContext context)
    {
        var dealerCode = context.DealerCode;
        var product = context.Product;

        if (!string.IsNullOrEmpty(dealerCode) && !string.IsNullOrEmpty(product))
        {
            return await CheckEligibilityAsync(dealerCode, product);
        }

        if (!string.IsNullOrEmpty(dealerCode) && string.IsNullOrEmpty(product))
        {
            var dealerName = _dealerService.GetDealerName(dealerCode);
            var nameDisplay = dealerName != null ? $" ({dealerName})" : "";
            var products = _productRegistry.GetAllProducts();
            var productList = string.Join(", ", products);

            return new ChatResponse
            {
                Content = $"I found dealer {dealerCode}{nameDisplay}. Which product would you like to check eligibility for?\n\nAvailable products: {productList}",
                Suggestions = products.Select(p => new SuggestedAction
                {
                    Label = p,
                    Action = $"Can {dealerCode} sell {p}?",
                    Payload = p
                }).ToList()
            };
        }

        if (string.IsNullOrEmpty(dealerCode) && !string.IsNullOrEmpty(product))
        {
            return new ChatResponse
            {
                Content = $"I can check eligibility for {product}. What is the dealer code? (e.g., BC006642, ON008800)",
                Suggestions =
                [
                    new() { Label = "BC006642", Action = $"Can BC006642 sell {product}?", Payload = "BC006642" },
                    new() { Label = "ON008800", Action = $"Can ON008800 sell {product}?", Payload = "ON008800" },
                ]
            };
        }

        return new ChatResponse
        {
            Content = "I can help check dealer eligibility. Please provide:\n• **Dealer Code** (e.g., BC006642)\n• **Product** (e.g., DW, EW, GAP)\n\nOr just ask something like: \"Can BC006642 sell DW?\"",
            Suggestions =
            [
                new() { Label = "Can BC006642 sell DW?", Action = "Can BC006642 sell DW?" },
                new() { Label = "Can ON008800 sell GAP?", Action = "Can ON008800 sell GAP?" },
                new() { Label = "Check BC001234 for EW", Action = "Can BC001234 sell EW?" },
            ]
        };
    }

    private async Task<ChatResponse> CheckEligibilityAsync(string dealerCode, string product)
    {
        var result = await _dealerService.CheckEligibilityAsync(dealerCode, product);

        List<SuggestedAction> suggestions;
        if (result.IsEligible)
        {
            suggestions =
            [
                new() { Label = "Check another product", Action = $"What else can {dealerCode} sell?" },
                new() { Label = "Check another dealer", Action = "Check dealer eligibility" },
            ];
        }
        else
        {
            suggestions =
            [
                new() { Label = "Activate this product", Action = $"Activate {product} for {dealerCode}" },
                new() { Label = "Check another product", Action = $"What else can {dealerCode} sell?" },
                new() { Label = "Check another dealer", Action = "Check dealer eligibility" },
            ];
        }

        return new ChatResponse
        {
            Content = result.Summary,
            EligibilityResult = result,
            Suggestions = suggestions
        };
    }

    private static bool HasEligibilityIntent(string message)
    {
        var lower = message.ToLowerInvariant();
        var intentKeywords = new[]
        {
            "sell", "eligible", "eligibility", "can", "does", "check",
            "activate", "setup", "set up", "dealer", "product", "program"
        };
        return intentKeywords.Count(k => lower.Contains(k)) >= 1 &&
               (lower.Contains("dealer") || lower.Contains("sell") || lower.Contains("eligible") || lower.Contains("check"));
    }
}
