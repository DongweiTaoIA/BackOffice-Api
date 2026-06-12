using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IDealerService
{
    Task<EligibilityResult> CheckEligibilityAsync(string dealerCode, string product);
    Task<EligibilityResult> CheckEligibilityAsync(EligibilityQuery query);
    Task<ProgramActionResult> ActivateProgramAsync(ActivateRequest request);
    Task<ProgramActionResult> DeactivateProgramAsync(DeactivateRequest request);
    Task<MaxMarkupResult> GetMaxMarkupAsync(string programIdOrName);
    Task<List<DealerSearchResult>> SearchDealersAsync(string query);
    Task<DealerPagedResult> GetDealersPagedAsync(int page, int pageSize, string? search, string? status, string? province, string? sortBy, string? sortDir);
    Task<DealerDetailsDto?> GetDealerDetailsAsync(string dealerCode);
    string? GetDealerName(string dealerCode);
}
