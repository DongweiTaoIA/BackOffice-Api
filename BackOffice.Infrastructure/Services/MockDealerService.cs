using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;

namespace BackOffice.Infrastructure.Services;

public class MockDealerService : IDealerService
{
    private static readonly Dictionary<string, DealerRecord> Dealers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BC006642"] = new("BC006642", "Pacific Auto Group", "Active", ["Dealer Warranty", "Extended Warranty", "GAP Premium"]),
        ["BC001234"] = new("BC001234", "Vancouver Motors", "Active", ["Dealer Warranty", "Extended Warranty", "Replacement Warranty", "Pre-Paid Maintenance"]),
        ["AB005500"] = new("AB005500", "Calgary Auto Centre", "Active", ["Extended Warranty", "GAP Premium", "Tire & Rim"]),
        ["AB003210"] = new("AB003210", "Edmonton Vehicle Sales", "Suspended", []),
        ["ON008800"] = new("ON008800", "Toronto Dealer Network", "Active", ["Dealer Warranty", "Extended Warranty", "GAP Premium", "Replacement Warranty", "Pre-Paid Maintenance", "Tire & Rim"]),
        ["ON007711"] = new("ON007711", "Ottawa Car Sales", "Active", ["Dealer Warranty", "Extended Warranty"]),
        ["QC004455"] = new("QC004455", "Montreal Auto Ventes", "Active", ["Dealer Warranty", "GAP Premium"]),
        ["SK002200"] = new("SK002200", "Saskatoon Motors", "Inactive", ["Dealer Warranty"]),
        ["MB001100"] = new("MB001100", "Winnipeg Auto Deals", "Active", ["Extended Warranty", "Replacement Warranty", "Tire & Rim"]),
        ["NS003300"] = new("NS003300", "Halifax Motor Group", "Active", ["Dealer Warranty", "Extended Warranty", "GAP Premium"]),
    };

    private static readonly Dictionary<string, List<ProgramInfo>> ProductPrograms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dealer Warranty"] =
        [
            new() { Code = "DW100", Name = "Dealer Warranty Basic", Status = "Active" },
            new() { Code = "DW200", Name = "Dealer Warranty Plus", Status = "Active" },
        ],
        ["Extended Warranty"] =
        [
            new() { Code = "AU220", Name = "Extended Warranty Standard", Status = "Active" },
            new() { Code = "AU330", Name = "Extended Warranty Premium", Status = "Active" },
            new() { Code = "AU110", Name = "Extended Warranty Lite", Status = "Discontinued" },
        ],
        ["GAP Premium"] =
        [
            new() { Code = "GP001", Name = "GAP Premium", Status = "Active" },
            new() { Code = "GP002", Name = "GAP Standard", Status = "Active" },
        ],
        ["Replacement Warranty"] =
        [
            new() { Code = "RW100", Name = "Replacement Warranty Full", Status = "Active" },
        ],
        ["Pre-Paid Maintenance"] =
        [
            new() { Code = "PPM01", Name = "Pre-Paid Maintenance 3yr", Status = "Active" },
            new() { Code = "PPM02", Name = "Pre-Paid Maintenance 5yr", Status = "Active" },
        ],
        ["Tire & Rim"] =
        [
            new() { Code = "TR001", Name = "Tire & Rim Protection", Status = "Active" },
        ],
    };

    public Task<EligibilityResult> CheckEligibilityAsync(string dealerCode, string product)
    {
        var result = new EligibilityResult
        {
            DealerCode = dealerCode.ToUpperInvariant(),
            Product = product,
            Environment = "FNCT",
        };

        if (!Dealers.TryGetValue(dealerCode, out var dealer))
        {
            result.IsEligible = false;
            result.DealerName = "Unknown";
            result.Summary = $"Dealer {dealerCode} was not found in the system.";
            return Task.FromResult(result);
        }

        result.DealerName = dealer.Name;

        if (dealer.Status != "Active")
        {
            result.IsEligible = false;
            result.Summary = $"Dealer {dealerCode} ({dealer.Name}) is currently {dealer.Status}. Cannot sell any products.";
            return Task.FromResult(result);
        }

        var canSell = dealer.EligibleProducts.Contains(product, StringComparer.OrdinalIgnoreCase);
        result.IsEligible = canSell;

        if (canSell)
        {
            result.Programs = ProductPrograms.TryGetValue(product, out var programs)
                ? programs.Where(p => p.Status == "Active").ToList()
                : [];
            result.Summary = $"Dealer {dealerCode} ({dealer.Name}) CAN sell {product}. {result.Programs.Count} active program(s) available.";
        }
        else
        {
            result.Programs = [];
            result.Summary = $"Dealer {dealerCode} ({dealer.Name}) CANNOT sell {product}. This product is not activated for this dealer.";
        }

        return Task.FromResult(result);
    }

    public string? GetDealerName(string dealerCode)
    {
        return Dealers.TryGetValue(dealerCode, out var dealer) ? dealer.Name : null;
    }

    private record DealerRecord(string Code, string Name, string Status, List<string> EligibleProducts);
}
