using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using BackOffice.Infrastructure.UnifiData;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class ClaimService(UnifiDbContext unifiDb) : IClaimService
{
    public async Task<ClaimPagedResult> GetClaimsPagedAsync(int page, int pageSize, string? search, string? status, string? claimType, string? dealerId, string? sortBy, string? sortDir)
    {
        var dbQuery =
            from cl in unifiDb.Claims.AsNoTracking()
            join ct in unifiDb.Contracts.AsNoTracking() on cl.ContractKey equals ct.ContractKey into ctj
            from ct in ctj.DefaultIfEmpty()
            join d in unifiDb.Dealers.AsNoTracking() on cl.ClaimDealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            select new
            {
                Claim = cl,
                ContractNum = ct != null ? ct.ContractNum : null,
                DealerName = d != null ? d.DBAName : null,
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.Like(x.Claim.ClaimNum, pattern)
                || (x.ContractNum != null && EF.Functions.Like(x.ContractNum, pattern))
                || (x.Claim.ExtClaimNum != null && EF.Functions.Like(x.Claim.ExtClaimNum, pattern))
                || (x.Claim.RONum != null && EF.Functions.Like(x.Claim.RONum, pattern))
                || (x.Claim.ClaimDealerId != null && EF.Functions.Like(x.Claim.ClaimDealerId, pattern))
                || (x.DealerName != null && EF.Functions.Like(x.DealerName, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            dbQuery = dbQuery.Where(x => x.Claim.ClaimStatPri == status.Trim());

        if (!string.IsNullOrWhiteSpace(claimType))
            dbQuery = dbQuery.Where(x => x.Claim.ClaimType == claimType.Trim());

        if (!string.IsNullOrWhiteSpace(dealerId))
        {
            var pattern = $"%{dealerId.Trim()}%";
            dbQuery = dbQuery.Where(x => x.Claim.ClaimDealerId != null && EF.Functions.Like(x.Claim.ClaimDealerId, pattern));
        }

        var totalCount = await dbQuery.CountAsync();

        var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        dbQuery = (sortBy?.ToLower()) switch
        {
            "claimnum" => isDesc ? dbQuery.OrderByDescending(x => x.Claim.ClaimNum) : dbQuery.OrderBy(x => x.Claim.ClaimNum),
            "dealerid" => isDesc ? dbQuery.OrderByDescending(x => x.Claim.ClaimDealerId) : dbQuery.OrderBy(x => x.Claim.ClaimDealerId),
            "claimtype" => isDesc ? dbQuery.OrderByDescending(x => x.Claim.ClaimType) : dbQuery.OrderBy(x => x.Claim.ClaimType),
            "status" => isDesc ? dbQuery.OrderByDescending(x => x.Claim.ClaimStatPri) : dbQuery.OrderBy(x => x.Claim.ClaimStatPri),
            "claimdt" => isDesc ? dbQuery.OrderByDescending(x => x.Claim.ClaimDt) : dbQuery.OrderBy(x => x.Claim.ClaimDt),
            "lossdt" => isDesc ? dbQuery.OrderByDescending(x => x.Claim.LossDt) : dbQuery.OrderBy(x => x.Claim.LossDt),
            _ => dbQuery.OrderByDescending(x => x.Claim.ClaimDt),
        };

        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClaimSearchResult
            {
                ClaimKey = x.Claim.ClaimKey,
                ClaimNum = x.Claim.ClaimNum,
                ContractKey = x.Claim.ContractKey,
                ContractNum = x.ContractNum,
                ClaimType = x.Claim.ClaimType,
                LossDt = x.Claim.LossDt,
                ClaimDt = x.Claim.ClaimDt,
                ClaimStatPri = x.Claim.ClaimStatPri,
                ClaimStatSec = x.Claim.ClaimStatSec,
                AdjusterId = x.Claim.AdjusterId,
                ClaimDealerId = x.Claim.ClaimDealerId,
                DealerName = x.DealerName,
                RONum = x.Claim.RONum,
                ExtClaimNum = x.Claim.ExtClaimNum,
            })
            .ToListAsync();

        return new ClaimPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            HasMore = page * pageSize < totalCount,
        };
    }

    public async Task<List<ClaimSearchResult>> SearchClaimsAsync(string query)
    {
        var q = (query ?? string.Empty).Trim();

        var dbQuery =
            from cl in unifiDb.Claims.AsNoTracking()
            join ct in unifiDb.Contracts.AsNoTracking() on cl.ContractKey equals ct.ContractKey into ctj
            from ct in ctj.DefaultIfEmpty()
            join d in unifiDb.Dealers.AsNoTracking() on cl.ClaimDealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            select new
            {
                Claim = cl,
                ContractNum = ct != null ? ct.ContractNum : null,
                DealerName = d != null ? d.DBAName : null,
            };

        if (!string.IsNullOrEmpty(q))
        {
            var pattern = $"%{q}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.Like(x.Claim.ClaimNum, pattern)
                || (x.ContractNum != null && EF.Functions.Like(x.ContractNum, pattern))
                || (x.Claim.ExtClaimNum != null && EF.Functions.Like(x.Claim.ExtClaimNum, pattern))
                || (x.Claim.RONum != null && EF.Functions.Like(x.Claim.RONum, pattern))
                || (x.Claim.ClaimDealerId != null && EF.Functions.Like(x.Claim.ClaimDealerId, pattern))
                || (x.DealerName != null && EF.Functions.Like(x.DealerName, pattern)));
        }

        return await dbQuery
            .OrderByDescending(x => x.Claim.ClaimDt)
            .ThenBy(x => x.Claim.ClaimNum)
            .Take(20)
            .Select(x => new ClaimSearchResult
            {
                ClaimKey = x.Claim.ClaimKey,
                ClaimNum = x.Claim.ClaimNum,
                ContractKey = x.Claim.ContractKey,
                ContractNum = x.ContractNum,
                ClaimType = x.Claim.ClaimType,
                LossDt = x.Claim.LossDt,
                ClaimDt = x.Claim.ClaimDt,
                ClaimStatPri = x.Claim.ClaimStatPri,
                ClaimStatSec = x.Claim.ClaimStatSec,
                AdjusterId = x.Claim.AdjusterId,
                ClaimDealerId = x.Claim.ClaimDealerId,
                DealerName = x.DealerName,
                RONum = x.Claim.RONum,
                ExtClaimNum = x.Claim.ExtClaimNum,
            })
            .ToListAsync();
    }

    public async Task<ClaimDetailsDto?> GetClaimDetailsAsync(string claimNum)
    {
        return await (
            from cl in unifiDb.Claims.AsNoTracking()
            join ct in unifiDb.Contracts.AsNoTracking() on cl.ContractKey equals ct.ContractKey into ctj
            from ct in ctj.DefaultIfEmpty()
            join d in unifiDb.Dealers.AsNoTracking() on cl.ClaimDealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            where cl.ClaimNum == claimNum
            select new ClaimDetailsDto
            {
                ClaimKey = cl.ClaimKey,
                ClaimNum = cl.ClaimNum,
                ContractKey = cl.ContractKey,
                ContractNum = ct != null ? ct.ContractNum : null,
                ClaimType = cl.ClaimType,
                LossDt = cl.LossDt,
                LossType = cl.LossType,
                ClaimDt = cl.ClaimDt,
                EClaimReadyDt = cl.EClaimReadyDt,
                SubmitDt = cl.SubmitDt,
                OpenDt = cl.OpenDt,
                ClaimStatPri = cl.ClaimStatPri,
                ClaimStatSec = cl.ClaimStatSec,
                AdjusterId = cl.AdjusterId,
                AdjudPriority = cl.AdjudPriority,
                AdjudStat = cl.AdjudStat,
                ClosedDt = cl.ClosedDt,
                NumOfKm = cl.NumOfKm,
                NumOfMiles = cl.NumOfMiles,
                LiabilityLimit = cl.LiabilityLimit,
                OverrideYN = cl.OverrideYN,
                OverrideBy = cl.OverrideBy,
                OverrideDt = cl.OverrideDt,
                LicensePlate = cl.LicensePlate,
                RONum = cl.RONum,
                ContactName = cl.ContactName,
                ContactPhoneNum = cl.ContactPhoneNum,
                ContactPhoneExt = cl.ContactPhoneExt,
                ContactFaxNum = cl.ContactFaxNum,
                ContactEmail = cl.ContactEmail,
                RepairCenter = cl.RepairCenter,
                Comments = cl.Comments,
                VerifyHistoryYN = cl.VerifyHistoryYN,
                VehicleHistoryVerifiedYN = cl.VehicleHistoryVerifiedYN,
                ExtClaimNum = cl.ExtClaimNum,
                ClaimDealerId = cl.ClaimDealerId,
                DealerName = d != null ? d.DBAName : null,
                PreferredLanguage = cl.PreferredLanguage,
                DataSource = cl.DataSource,
                CreatedBy = cl.CreatedBy,
                CreatedDt = cl.CreatedDt,
                ModDtTime = cl.ModDtTime,
                ModLoginId = cl.ModLoginId,
            }).FirstOrDefaultAsync();
    }
}
