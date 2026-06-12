using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IContractService
{
    Task<List<ContractSearchResult>> SearchContractsAsync(string query);
    Task<ContractDetailsDto?> GetContractDetailsAsync(string contractNum);
}
