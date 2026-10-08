using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IContractService
{
    Task<List<ContractSearchResult>> SearchContractsAsync(string query);
    Task<ContractPagedResult> GetContractsPagedAsync(int page, int pageSize, string? search, string? status, string? product, string? dealerId, string? sortBy, string? sortDir);
    Task<ContractDetailsDto?> GetContractDetailsAsync(string contractNum);
    Task<ContractStatusDto?> GetContractStatusAsync(string contractNum);
    Task<CancellationEligibilityResult?> CheckCancellationEligibilityAsync(
        string contractNum,
        DateTime cancellationDate,
        string? ruleId,
        string? cancType,
        string? lang,
        string? userId);
}
