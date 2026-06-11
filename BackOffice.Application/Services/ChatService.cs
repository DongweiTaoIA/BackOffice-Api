using System.Text.RegularExpressions;
using BackOffice.Application.Services.Intents;
using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BackOffice.Application.Services;

/// <summary>
/// Routes an incoming user message to the highest-scoring intent handler.
/// Adding a new capability only requires implementing <see cref="IChatIntentHandler"/>
/// and registering it in DI — this class does not need to change.
/// </summary>
public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly IProductRegistry _productRegistry;
    private readonly IEnumerable<IChatIntentHandler> _handlers;

    public ChatService(
        ILogger<ChatService> logger,
        IProductRegistry productRegistry,
        IEnumerable<IChatIntentHandler> handlers)
    {
        _logger = logger;
        _productRegistry = productRegistry;
        _handlers = handlers;
    }

    public async Task<MessageDto> ProcessMessageAsync(string chatId, string userMessage)
    {
        _logger.LogInformation("Processing message for chat {ChatId}", chatId);

        var context = BuildContext(userMessage);

        // Pick the highest-scoring handler. FallbackIntentHandler always scores 1,
        // so there is always at least one handler selected.
        var handler = _handlers
            .Select(h => (Handler: h, Score: h.Score(context)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .First()
            .Handler;

        _logger.LogInformation("Routing message to {Handler}", handler.GetType().Name);

        ChatResponse response;
        try
        {
            response = await handler.HandleAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handler {Handler} failed", handler.GetType().Name);
            response = new ChatResponse
            {
                Content = "Something went wrong while processing your request. Please try again."
            };
        }

        return new MessageDto
        {
            Id = Guid.NewGuid().ToString(),
            ChatId = chatId,
            Role = "assistant",
            Content = response.Content,
            Timestamp = DateTime.UtcNow,
            EligibilityResult = response.EligibilityResult,
            Suggestions = response.Suggestions
        };
    }

    private ChatContext BuildContext(string message)
    {
        var dealerCode = ExtractDealerCode(message);
        var productToken = ExtractProductToken(message);
        var resolvedProduct = _productRegistry.ResolveProduct(productToken);

        // Detect if the user specified a program (by code or name) after the product
        string? programCode = null;

        if (!string.IsNullOrEmpty(resolvedProduct))
        {
            // Try to extract program reference from the message
            // Pattern: after the product shortcode (e.g., "EW"), remaining text may be a program name
            var programRef = ExtractProgramRef(message, dealerCode, resolvedProduct);
            if (!string.IsNullOrEmpty(programRef))
            {
                programCode = programRef;
            }
        }

        return new()
        {
            Message = message,
            DealerCode = dealerCode,
            Product = resolvedProduct,
            ProgramCode = programCode,
        };
    }

    private static string? ExtractProgramRef(string message, string? dealerCode, string productId)
    {
        // Remove known parts from the message to find program name
        var cleaned = message;

        // Remove common intent words
        string[] removeWords = ["can", "does", "is", "sell", "check", "eligible", "eligibility", "for", "the", "?"];
        foreach (var word in removeWords)
        {
            cleaned = Regex.Replace(cleaned, $@"(?<![a-zA-Z]){Regex.Escape(word)}(?![a-zA-Z])", " ", RegexOptions.IgnoreCase);
        }

        // Remove dealer code
        if (!string.IsNullOrEmpty(dealerCode))
        {
            cleaned = cleaned.Replace(dealerCode, " ", StringComparison.OrdinalIgnoreCase);
        }

        // Remove product shortcode (standalone, e.g., "EW" but not part of a word)
        cleaned = Regex.Replace(cleaned, $@"(?<![a-zA-Z]){Regex.Escape(productId)}(?![a-zA-Z])", " ", RegexOptions.IgnoreCase);

        // Clean up whitespace
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        // If there's meaningful text left (at least 3 chars), it could be a program code or name
        if (cleaned.Length >= 3)
        {
            return cleaned;
        }

        // Also check for explicit program codes like "AU220" in original message
        var codeMatch = Regex.Match(message, @"[A-Za-z]{2}\d{2,3}");
        if (codeMatch.Success)
        {
            var code = codeMatch.Value.ToUpperInvariant();
            // Make sure it's not the dealer code
            if (dealerCode == null || !code.Equals(dealerCode[..Math.Min(5, dealerCode.Length)], StringComparison.OrdinalIgnoreCase))
            {
                // And not the product code
                if (!code.Equals(productId, StringComparison.OrdinalIgnoreCase))
                    return code;
            }
        }

        return null;
    }

    private static string? ExtractDealerCode(string message)
    {
        var match = Regex.Match(message, @"[A-Za-z]{2}\s?\d{4,6}");
        if (match.Success)
        {
            return match.Value.Replace(" ", "").ToUpperInvariant();
        }
        return null;
    }

    private string ExtractProductToken(string message)
    {
        var resolved = _productRegistry.ResolveProduct(message);
        if (resolved != null)
            return message;

        var lower = message.ToLowerInvariant();

        var codeMatch = Regex.Match(message, @"[A-Za-z]{2}\d{2,3}", RegexOptions.IgnoreCase);
        if (codeMatch.Success)
        {
            var code = codeMatch.Value.ToLowerInvariant();
            if (!Regex.IsMatch(codeMatch.Value, @"^[A-Za-z]{2}\d{4,6}$"))
            {
                var fromCode = _productRegistry.ResolveProduct(code);
                if (fromCode != null)
                    return code;
            }
        }

        string[] shortCodes = ["dw", "ew", "rw", "gap", "ppm", "tr"];
        foreach (var code in shortCodes)
        {
            if (Regex.IsMatch(lower, $@"(?<![a-z]){Regex.Escape(code)}(?![a-z])"))
                return code;
        }

        string[] phrases = [
            "dealer warranty", "extended warranty", "ext warranty",
            "replacement warranty", "gap premium", "gap insurance",
            "pre-paid maintenance", "prepaid maintenance",
            "tire and rim", "tire & rim", "tire rim",
            "dealer warrenty", "extended warrenty"
        ];
        foreach (var phrase in phrases.OrderByDescending(p => p.Length))
        {
            if (lower.Contains(phrase))
                return phrase;
        }

        return string.Empty;
    }
}
