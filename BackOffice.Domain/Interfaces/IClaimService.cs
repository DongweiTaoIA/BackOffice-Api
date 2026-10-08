using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IClaimService
{
    Task<List<ClaimSearchResult>> SearchClaimsAsync(string query);
    Task<ClaimPagedResult> GetClaimsPagedAsync(int page, int pageSize, string? search, string? status, string? claimType, string? dealerId, string? sortBy, string? sortDir);
    Task<ClaimDetailsDto?> GetClaimDetailsAsync(string claimNum);
}
