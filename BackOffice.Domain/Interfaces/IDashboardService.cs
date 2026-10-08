namespace BackOffice.Domain.Interfaces;

public interface IDashboardService
{
    Task<int> GetTotalActiveDealersAsync();
    Task<int> GetTotalProductsAsync();
    Task<int> GetTotalActiveProgramsAsync();
    Task<int> GetTotalActiveContractsAsync();
    Task<int> GetTotalActiveCustomersAsync();
    Task<int> GetTotalActiveVehiclesAsync();
}
