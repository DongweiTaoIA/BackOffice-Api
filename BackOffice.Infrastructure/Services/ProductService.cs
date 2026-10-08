using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using BackOffice.Infrastructure.UnifiData;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class ProductService(UnifiDbContext unifiDb) : IProductService
{
    public async Task<List<ProgramLookupResult>> SearchProgramsAsync(string query)
    {
        var q = (query ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(q))
            return [];

        // Base join mirrors:
        //   SELECT p.ProgramId, p.DescnEn, cg.ContractGroup, pr.ProductId, pr.DescnEn
        //   FROM   cfProgram p
        //   JOIN   cfContractGroup cg ON cg.ContractGroup = p.ContractGroup
        //   JOIN   cfProduct       pr ON pr.ProductId     = cg.ProductId
        var baseQuery =
            from p in unifiDb.Programs.AsNoTracking()
            join cg in unifiDb.ContractGroups.AsNoTracking() on p.ContractGroup equals cg.ContractGroup
            join pr in unifiDb.Products.AsNoTracking() on cg.ProductId equals pr.ProductId
            select new { p, cg, pr };

        // 1) Try EXACT match first (program code or full English/French name).
        //    A program name like "Retail Wearable Parts" can appear as a
        //    substring inside many other programs across multiple products;
        //    returning all of them produces an ambiguous result and the
        //    client can't tell which product owns the program the user
        //    actually meant. If we get an exact hit, only return that.
        var exact = await baseQuery
            .Where(x => x.p.ProgramId == q
                        || x.p.DescnEn == q
                        || (x.p.DescnFr != null && x.p.DescnFr == q))
            .OrderBy(x => x.p.SortOrder)
            .ThenBy(x => x.p.ProgramId)
            .Take(50)
            .Select(x => new ProgramLookupResult
            {
                ProgramId = x.p.ProgramId,
                ProgramName = x.p.DescnEn,
                ProgramNameFr = x.p.DescnFr,
                ContractGroup = x.cg.ContractGroup,
                ProductId = x.pr.ProductId,
                ProductName = x.pr.DescnEn,
            })
            .ToListAsync();

        if (exact.Count > 0)
            return exact;

        // 2) Fallback: code prefix or name contains. Ambiguous results are
        //    possible here; the client picks the best one.
        var partial = await baseQuery
            .Where(x => x.p.ProgramId.StartsWith(q)
                        || x.p.DescnEn.Contains(q)
                        || (x.p.DescnFr != null && x.p.DescnFr.Contains(q)))
            .OrderBy(x => x.p.SortOrder)
            .ThenBy(x => x.p.ProgramId)
            .Take(50)
            .Select(x => new ProgramLookupResult
            {
                ProgramId = x.p.ProgramId,
                ProgramName = x.p.DescnEn,
                ProgramNameFr = x.p.DescnFr,
                ContractGroup = x.cg.ContractGroup,
                ProductId = x.pr.ProductId,
                ProductName = x.pr.DescnEn,
            })
            .ToListAsync();

        return partial;
    }
}
