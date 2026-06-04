using BackOffice.Domain.Interfaces;

namespace BackOffice.Application.Services;

public class ProductRegistry : IProductRegistry
{
    private static readonly Dictionary<string, string> ExactAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // Dealer Warranty
        ["dw"] = "Dealer Warranty",
        ["dealer warranty"] = "Dealer Warranty",
        ["dealer war"] = "Dealer Warranty",

        // Extended Warranty
        ["ew"] = "Extended Warranty",
        ["extended warranty"] = "Extended Warranty",
        ["ext warranty"] = "Extended Warranty",
        ["extended"] = "Extended Warranty",
        ["au220"] = "Extended Warranty",
        ["au 220"] = "Extended Warranty",

        // GAP Premium
        ["gap"] = "GAP Premium",
        ["gap premium"] = "GAP Premium",
        ["gap insurance"] = "GAP Premium",

        // Replacement Warranty
        ["rw"] = "Replacement Warranty",
        ["replacement warranty"] = "Replacement Warranty",
        ["replacement"] = "Replacement Warranty",

        // PPM (Pre-Paid Maintenance)
        ["ppm"] = "Pre-Paid Maintenance",
        ["pre-paid maintenance"] = "Pre-Paid Maintenance",
        ["prepaid maintenance"] = "Pre-Paid Maintenance",
        ["maintenance"] = "Pre-Paid Maintenance",

        // Tire & Rim
        ["tr"] = "Tire & Rim",
        ["tire and rim"] = "Tire & Rim",
        ["tire & rim"] = "Tire & Rim",
        ["tire rim"] = "Tire & Rim",
    };

    private static readonly Dictionary<string, string> FuzzyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dealer warrenty"] = "Dealer Warranty",
        ["dealer warenty"] = "Dealer Warranty",
        ["dealer waranty"] = "Dealer Warranty",
        ["extended warrenty"] = "Extended Warranty",
        ["extended warenty"] = "Extended Warranty",
        ["replacement warrenty"] = "Replacement Warranty",
        ["gap premum"] = "GAP Premium",
        ["gap premuim"] = "GAP Premium",
    };

    public string? ResolveProduct(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var trimmed = input.Trim();

        if (ExactAliases.TryGetValue(trimmed, out var exact))
            return exact;

        if (FuzzyAliases.TryGetValue(trimmed, out var fuzzy))
            return fuzzy;

        foreach (var (alias, product) in ExactAliases.OrderByDescending(k => k.Key.Length))
        {
            if (trimmed.Contains(alias, StringComparison.OrdinalIgnoreCase))
                return product;
        }

        foreach (var (alias, product) in FuzzyAliases.OrderByDescending(k => k.Key.Length))
        {
            if (trimmed.Contains(alias, StringComparison.OrdinalIgnoreCase))
                return product;
        }

        return null;
    }

    public List<string> GetAllProducts() =>
    [
        "Dealer Warranty",
        "Extended Warranty",
        "GAP Premium",
        "Replacement Warranty",
        "Pre-Paid Maintenance",
        "Tire & Rim"
    ];
}
