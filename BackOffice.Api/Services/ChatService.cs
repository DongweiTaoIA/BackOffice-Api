using BackOffice.Api.Models;

namespace BackOffice.Api.Services;

public interface IChatService
{
    Task<MessageDto> ProcessMessageAsync(string conversationId, string userMessage);
}

public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;

    public ChatService(ILogger<ChatService> logger)
    {
        _logger = logger;
    }

    public async Task<MessageDto> ProcessMessageAsync(string conversationId, string userMessage)
    {
        _logger.LogInformation("Processing message for conversation {ConversationId}", conversationId);

        // Simulate AI processing delay
        await Task.Delay(500);

        var response = GenerateResponse(userMessage);

        return new MessageDto
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversationId,
            Role = "assistant",
            Content = response.Content,
            Timestamp = DateTime.UtcNow,
            EligibilityResult = response.EligibilityResult
        };
    }

    private (string Content, EligibilityResult? EligibilityResult) GenerateResponse(string userMessage)
    {
        var lowerMessage = userMessage.ToLowerInvariant();

        // Dealer eligibility check pattern
        if (lowerMessage.Contains("dealer") && (lowerMessage.Contains("sell") || lowerMessage.Contains("eligible")))
        {
            var dealerCode = ExtractDealerCode(userMessage);
            var product = ExtractProduct(userMessage);

            if (!string.IsNullOrEmpty(dealerCode) && !string.IsNullOrEmpty(product))
            {
                var result = new EligibilityResult
                {
                    IsEligible = true,
                    DealerCode = dealerCode,
                    DealerName = "Pacific Auto Group",
                    Product = product,
                    Environment = "FNCT",
                    Programs =
                    [
                        new ProgramInfo { Code = "GP001", Name = $"{product} Premium", Status = "Active" },
                        new ProgramInfo { Code = "GP003", Name = $"{product} Standard", Status = "Active" }
                    ],
                    Summary = $"Dealer {dealerCode} (Pacific Auto Group) CAN sell {product}. 2 active program(s) available."
                };

                return ($"Dealer {dealerCode} eligibility check complete.", result);
            }
        }

        // Default response
        return ("I'm Team PnC, your BackOffice assistant. I can help with dealer eligibility checks, operations management, and more. How can I help you today?", null);
    }

    private static string ExtractDealerCode(string message)
    {
        var match = System.Text.RegularExpressions.Regex.Match(message, @"BC\d{6}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Value.ToUpperInvariant() : string.Empty;
    }

    private static string ExtractProduct(string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        if (lowerMessage.Contains("gap")) return "GAP Premium";
        if (lowerMessage.Contains("ew") || lowerMessage.Contains("extended warranty")) return "Extended Warranty";
        if (lowerMessage.Contains("dw")) return "Dealer Warranty";
        if (lowerMessage.Contains("rw")) return "Replacement Warranty";
        return string.Empty;
    }
}
