using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IDealerService
{
    Task<EligibilityResult> CheckEligibilityAsync(string dealerCode, string product);
    Task<EligibilityResult> CheckEligibilityAsync(EligibilityQuery query);
    Task<ProgramActionResult> ActivateProgramAsync(ActivateRequest request);
    Task<ProgramActionResult> DeactivateProgramAsync(DeactivateRequest request);
    Task<MaxMarkupResult> GetMaxMarkupAsync(string programIdOrName);
    string? GetDealerName(string dealerCode);
}
