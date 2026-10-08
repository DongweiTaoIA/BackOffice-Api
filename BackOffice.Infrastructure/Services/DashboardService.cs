using BackOffice.Domain.Interfaces;
using BackOffice.Infrastructure.UnifiData;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class DashboardService(UnifiDbContext unifiDb) : IDashboardService
{
    public Task<int> GetTotalActiveDealersAsync()
    {
        return unifiDb.Dealers
            .AsNoTracking()
            .CountAsync(d => d.IsDemoYN == "N" && d.DealerStat == "A");
    }

    public Task<int> GetTotalProductsAsync()
    {
        // Count rows in the cfProduct catalog (one row per product such as EW, DW, GP, ...).
        return unifiDb.Products
            .AsNoTracking()
            .CountAsync();
    }

    public Task<int> GetTotalActiveProgramsAsync()
    {
        var dwIds =
            from dw in unifiDb.DwContracts.AsNoTracking()
            join c in unifiDb.Contracts.AsNoTracking() on dw.ContractKey equals c.ContractKey
            where c.ContractStatPri == "A"
                && c.ProductId == "DW"
                && dw.DwProgramId != null
            select dw.DwProgramId!;

        var gpIds =
            from gp in unifiDb.GpContracts.AsNoTracking()
            join c in unifiDb.Contracts.AsNoTracking() on gp.ContractKey equals c.ContractKey
            where c.ContractStatPri == "A"
                && c.ProductId == "GP"
                && gp.GpProgramId != null
            select gp.GpProgramId!;

        var otherIds = unifiDb.Contracts
            .AsNoTracking()
            .Where(c => c.ContractStatPri == "A"
                && c.ProductId != "DW"
                && c.ProductId != "GP"
                && c.ProgramId != null)
            .Select(c => c.ProgramId!);

        // Union (not UnionAll) performs DISTINCT, matching COUNT(DISTINCT ProgramId) in the source SQL.
        return dwIds.Union(gpIds).Union(otherIds).CountAsync();
    }

    public Task<int> GetTotalActiveContractsAsync()
    {
        return unifiDb.Contracts
            .AsNoTracking()
            .CountAsync(c => c.ContractStatPri == "A");
    }

    public Task<int> GetTotalActiveCustomersAsync()
    {
        var customer1Keys = unifiDb.Contracts
            .AsNoTracking()
            .Where(c => c.ContractStatPri == "A" && c.Customer1Key != null)
            .Select(c => c.Customer1Key!.Value);

        var customer2Keys = unifiDb.Contracts
            .AsNoTracking()
            .Where(c => c.ContractStatPri == "A" && c.Customer2Key != null)
            .Select(c => c.Customer2Key!.Value);

        // Union (not UnionAll) performs DISTINCT, matching COUNT(DISTINCT CustomerKey) in the source SQL.
        return customer1Keys.Union(customer2Keys).CountAsync();
    }

    public Task<int> GetTotalActiveVehiclesAsync()
    {
        return unifiDb.Contracts
            .AsNoTracking()
            .Where(c => c.ContractStatPri == "A" && c.VehicleKey != null)
            .Select(c => c.VehicleKey!.Value)
            .Distinct()
            .CountAsync();
    }
}
