using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IDealerService
{
    Task<EligibilityResult> CheckEligibilityAsync(string dealerCode, string product);
    string? GetDealerName(string dealerCode);
}
