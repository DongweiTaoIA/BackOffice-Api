using BackOffice.Domain.Interfaces;

namespace BackOffice.Application.Services;

public class ProductRegistry : IProductRegistry
{
    // Maps user input aliases to cfProduct.ProductId codes (CHAR(2))
    private static readonly Dictionary<string, string> ExactAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // Dealer Warranty
        ["dw"] = "DW",
        ["dealer warranty"] = "DW",
        ["dealer war"] = "DW",

        // Extended Warranty
        ["ew"] = "EW",
        ["extended warranty"] = "EW",
        ["ext warranty"] = "EW",
        ["extended"] = "EW",
        ["au220"] = "EW",
        ["au 220"] = "EW",
        ["retail wearable parts"] = "EW",

        // GAP Premium
        ["gp"] = "GP",
        ["gap"] = "GP",
        ["gap premium"] = "GP",
        ["gap insurance"] = "GP",

        // Replacement Warranty
        ["rw"] = "RW",
        ["replacement warranty"] = "RW",
        ["replacement"] = "RW",

        // Pre-Paid Maintenance
        ["pm"] = "PM",
        ["ppm"] = "PM",
        ["pre-paid maintenance"] = "PM",
        ["prepaid maintenance"] = "PM",
        ["maintenance"] = "PM",

        // Tire & Rim
        ["tr"] = "TR",
        ["tire and rim"] = "TR",
        ["tire & rim"] = "TR",
        ["tire rim"] = "TR",
    };

    private static readonly Dictionary<string, string> FuzzyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dealer warrenty"] = "DW",
        ["dealer warenty"] = "DW",
        ["dealer waranty"] = "DW",
        ["extended warrenty"] = "EW",
        ["extended warenty"] = "EW",
        ["replacement warrenty"] = "RW",
        ["gap premum"] = "GP",
        ["gap premuim"] = "GP",
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
        "DW",
        "EW",
        "GP",
        "RW",
        "PM",
        "TR"
    ];
}
