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
        var language = context.Language;

        if (!string.IsNullOrEmpty(dealerCode) && !string.IsNullOrEmpty(product))
        {
            // If a specific program was identified, use the query overload for precise checking
            if (!string.IsNullOrEmpty(context.ProgramCode))
            {
                var query = new EligibilityQuery
                {
                    DealerId = dealerCode,
                    ProductId = product,
                    ProgramName = context.ProgramCode,
                };
                var queryResult = await _dealerService.CheckEligibilityAsync(query);
                return BuildResponse(queryResult, language);
            }

            return await CheckEligibilityAsync(dealerCode, product, language);
        }

        if (!string.IsNullOrEmpty(dealerCode) && string.IsNullOrEmpty(product))
        {
            var dealerName = _dealerService.GetDealerName(dealerCode);
            var products = _productRegistry.GetAllProducts();
            var productList = string.Join(", ", products);

            return new ChatResponse
            {
                Content = ChatLocalizer.AskProductForDealer(language, dealerCode, dealerName, productList),
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
                Content = ChatLocalizer.AskDealerForProduct(language, product),
                Suggestions =
                [
                    new() { Label = "BC006642", Action = $"Can BC006642 sell {product}?", Payload = "BC006642" },
                    new() { Label = "ON008800", Action = $"Can ON008800 sell {product}?", Payload = "ON008800" },
                ]
            };
        }

        return new ChatResponse
        {
            Content = ChatLocalizer.EligibilityGenericPrompt(language),
            Suggestions =
            [
                new() { Label = ChatLocalizer.LabelCanSell(language, "BC006642", "DW"), Action = "Can BC006642 sell DW?" },
                new() { Label = ChatLocalizer.LabelCanSell(language, "ON008800", "GAP"), Action = "Can ON008800 sell GAP?" },
                new() { Label = ChatLocalizer.LabelCheckDealerForProduct(language, "BC001234", "EW"), Action = "Can BC001234 sell EW?" },
            ]
        };
    }

    private async Task<ChatResponse> CheckEligibilityAsync(string dealerCode, string product, string language)
    {
        var result = await _dealerService.CheckEligibilityAsync(dealerCode, product);
        return BuildResponse(result, language, dealerCode, product);
    }

    private static ChatResponse BuildResponse(EligibilityResult result, string language, string? dealerCode = null, string? product = null)
    {
        dealerCode ??= result.DealerCode;
        product ??= result.Product;

        // Overwrite the English summary produced by DealerService with a localized version.
        result.Summary = LocalizeSummary(result, language);

        List<SuggestedAction> suggestions;
        if (result.IsEligible)
        {
            suggestions =
            [
                new() { Label = ChatLocalizer.LabelCheckAnotherProduct(language), Action = $"What else can {dealerCode} sell?" },
                new() { Label = ChatLocalizer.LabelCheckAnotherDealer(language), Action = "Check dealer eligibility" },
            ];
        }
        else
        {
            suggestions =
            [
                new() { Label = ChatLocalizer.LabelActivateProduct(language), Action = $"Activate {product} for {dealerCode}" },
                new() { Label = ChatLocalizer.LabelCheckAnotherProduct(language), Action = $"What else can {dealerCode} sell?" },
                new() { Label = ChatLocalizer.LabelCheckAnotherDealer(language), Action = "Check dealer eligibility" },
            ];
        }

        return new ChatResponse
        {
            Content = result.Summary,
            EligibilityResult = result,
            Suggestions = suggestions
        };
    }

    private static string LocalizeSummary(EligibilityResult result, string language)
    {
        // Dealer not found: DealerService sets DealerName="Unknown" and leaves DealerStatus null.
        if (string.IsNullOrEmpty(result.DealerStatus)
            && string.Equals(result.DealerName, "Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return ChatLocalizer.DealerNotFound(language, result.DealerCode);
        }

        // Dealer inactive: status is set but not "A".
        if (!string.IsNullOrEmpty(result.DealerStatus)
            && !string.Equals(result.DealerStatus, "A", StringComparison.OrdinalIgnoreCase))
        {
            return ChatLocalizer.DealerInactive(language, result.DealerCode, result.DealerName, result.DealerStatus);
        }

        if (!result.IsEligible)
        {
            return ChatLocalizer.CannotSellProduct(language, result.DealerCode, result.DealerName, result.Product);
        }

        // Eligible — if exactly one program matched the user's filter, use the program-specific phrasing.
        if (result.Programs.Count == 1)
        {
            var p = result.Programs[0];
            var localizedName = LanguageDetector.IsFrench(language) && !string.IsNullOrWhiteSpace(p.NameFr)
                ? p.NameFr!
                : p.Name;
            return ChatLocalizer.CanSellUnderProgram(language, result.DealerCode, result.DealerName, result.Product, p.Code, localizedName);
        }

        return ChatLocalizer.CanSellProduct(language, result.DealerCode, result.DealerName, result.Product, result.Programs.Count);
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
