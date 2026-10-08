using System.Text.RegularExpressions;
using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;

namespace BackOffice.Application.Services.Intents;

/// <summary>
/// Handles "look up a dealer" intents like "search BC006642", "chercher BC006642",
/// "find Driveco", "show me ON008800". Returns a localized "Found dealer ..." message
/// plus eligibility-check suggestions for common products.
///
/// Scoring rule of thumb:
/// - Requires an explicit lookup verb (English or French) to fire at all.
/// - Defers to <see cref="EligibilityIntentHandler"/> when a product is also mentioned.
/// </summary>
public class DealerLookupIntentHandler : IChatIntentHandler
{
    private readonly IDealerService _dealerService;

    public DealerLookupIntentHandler(IDealerService dealerService)
    {
        _dealerService = dealerService;
    }

    public int Score(ChatContext context)
    {
        // Require an explicit lookup verb. Without it, defer to other handlers.
        if (!HasLookupIntent(context.Message))
            return 0;

        // If the user also referenced a product, that's an eligibility question, not a lookup.
        if (!string.IsNullOrEmpty(context.Product))
            return 0;

        var score = 3; // base score for a clear lookup verb
        if (!string.IsNullOrEmpty(context.DealerCode))
            score += 3;
        return score;
    }

    public async Task<ChatResponse> HandleAsync(ChatContext context)
    {
        var language = context.Language;

        // Case 1: dealer code in the message → direct lookup.
        if (!string.IsNullOrEmpty(context.DealerCode))
        {
            var details = await _dealerService.GetDealerDetailsAsync(context.DealerCode);
            if (details == null)
            {
                return new ChatResponse
                {
                    Content = ChatLocalizer.DealerNotFound(language, context.DealerCode)
                };
            }

            return new ChatResponse
            {
                Content = ChatLocalizer.DealerLookupFound(language, details.DealerId, details.DBAName),
                Suggestions = BuildPostLookupSuggestions(language, details.DealerId)
            };
        }

        // Case 2: free-text search (e.g. "chercher Driveco").
        var searchTerm = ExtractSearchTerm(context.Message);
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new ChatResponse
            {
                Content = ChatLocalizer.AskDealerToLookup(language)
            };
        }

        var results = await _dealerService.SearchDealersAsync(searchTerm);

        if (results.Count == 0)
        {
            return new ChatResponse
            {
                Content = ChatLocalizer.DealerLookupNoResults(language, searchTerm)
            };
        }

        if (results.Count == 1)
        {
            var d = results[0];
            return new ChatResponse
            {
                Content = ChatLocalizer.DealerLookupFound(language, d.DealerId, d.DBAName),
                Suggestions = BuildPostLookupSuggestions(language, d.DealerId)
            };
        }

        // Multiple matches → ask the user to pick.
        return new ChatResponse
        {
            Content = ChatLocalizer.DealerLookupMultiple(language, results.Count, searchTerm),
            Suggestions = results
                .Take(5)
                .Select(d => new SuggestedAction
                {
                    Label = $"{d.DealerId} — {d.DBAName}",
                    Action = $"Show {d.DealerId}",
                    Payload = d.DealerId
                })
                .ToList()
        };
    }

    private static List<SuggestedAction> BuildPostLookupSuggestions(string language, string dealerCode) =>
    [
        new() { Label = ChatLocalizer.LabelCanSell(language, dealerCode, "DW"), Action = $"Can {dealerCode} sell DW?" },
        new() { Label = ChatLocalizer.LabelCanSell(language, dealerCode, "EW"), Action = $"Can {dealerCode} sell EW?" },
        new() { Label = ChatLocalizer.LabelCanSell(language, dealerCode, "GAP"), Action = $"Can {dealerCode} sell GAP?" },
    ];

    // English + French lookup verbs. Whole-word match (case-insensitive).
    private static readonly string[] LookupKeywords =
    [
        // English
        "search", "find", "lookup", "look up", "show", "show me", "who is", "details for", "details",
        // French
        "chercher", "cherche", "rechercher", "recherche", "trouver", "trouve",
        "montrer", "montre", "afficher", "affiche", "voir", "détails"
    ];

    private static readonly Regex LookupRegex = new(
        @"(?<![\p{L}])(" + string.Join("|", LookupKeywords.Select(Regex.Escape)) + @")(?![\p{L}])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static bool HasLookupIntent(string message) =>
        !string.IsNullOrWhiteSpace(message) && LookupRegex.IsMatch(message);

    // Words to strip when extracting a free-text search term.
    private static readonly string[] StripWords =
    [
        // Lookup verbs themselves
        "search", "find", "lookup", "look up", "show", "show me", "who is", "details for", "details",
        "chercher", "cherche", "rechercher", "recherche", "trouver", "trouve",
        "montrer", "montre", "afficher", "affiche", "voir", "détails",
        // Common filler words
        "for", "the", "a", "an", "dealer", "dealers",
        "pour", "le", "la", "les", "un", "une", "des", "du", "concessionnaire", "concessionnaires",
        "me", "moi"
    ];

    private static readonly Regex StripRegex = new(
        @"(?<![\p{L}])(" + string.Join("|", StripWords.Select(Regex.Escape)) + @")(?![\p{L}])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static string ExtractSearchTerm(string message)
    {
        var cleaned = StripRegex.Replace(message, " ");
        cleaned = cleaned.Replace("?", " ").Replace(".", " ").Replace(",", " ").Replace("!", " ");
        return WhitespaceRegex.Replace(cleaned, " ").Trim();
    }
}
