using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using BackOffice.Infrastructure.UnifiData;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class ContractService(UnifiDbContext unifiDb) : IContractService
{
    public async Task<List<ContractSearchResult>> SearchContractsAsync(string query)
    {
        var q = (query ?? string.Empty).Trim();

        var dbQuery =
            from c in unifiDb.Contracts.AsNoTracking()
            join d in unifiDb.Dealers.AsNoTracking() on c.DealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            select new { Contract = c, DealerName = d != null ? d.DBAName : null };

        if (!string.IsNullOrEmpty(q))
        {
            var pattern = $"%{q}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.Like(x.Contract.ContractNum, pattern)
                || EF.Functions.Like(x.Contract.DealerId, pattern)
                || (x.Contract.ExtContractNum != null && EF.Functions.Like(x.Contract.ExtContractNum, pattern))
                || (x.DealerName != null && EF.Functions.Like(x.DealerName, pattern)));
        }

        return await dbQuery
            .OrderByDescending(x => x.Contract.EffectDt)
            .ThenBy(x => x.Contract.ContractNum)
            .Take(20)
            .Select(x => new ContractSearchResult
            {
                ContractKey = x.Contract.ContractKey,
                ContractNum = x.Contract.ContractNum,
                ProductId = x.Contract.ProductId,
                DealerId = x.Contract.DealerId,
                DealerName = x.DealerName,
                ContractStatPri = x.Contract.ContractStatPri,
                ContractStatSec = x.Contract.ContractStatSec,
                EffectDt = x.Contract.EffectDt,
                ExpiryDt = x.Contract.ExpiryDt,
                ExtContractNum = x.Contract.ExtContractNum,
            })
            .ToListAsync();
    }

    public async Task<ContractDetailsDto?> GetContractDetailsAsync(string contractNum)
    {
        return await (
            from c in unifiDb.Contracts.AsNoTracking()
            join d in unifiDb.Dealers.AsNoTracking() on c.DealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            where c.ContractNum == contractNum
            select new ContractDetailsDto
            {
                ContractKey = c.ContractKey,
                ContractNum = c.ContractNum,
                CompanyId = c.CompanyId,
                ProductId = c.ProductId,
                DealerId = c.DealerId,
                DealerName = d != null ? d.DBAName : null,
                ContractType = c.ContractType,
                ProgramId = c.ProgramId,
                ContractStatPri = c.ContractStatPri,
                ContractStatSec = c.ContractStatSec,
                CreateDt = c.CreateDt,
                CreatedBy = c.CreatedBy,
                CalcDt = c.CalcDt,
                FinalDt = c.FinalDt,
                FinalBy = c.FinalBy,
                EffectDt = c.EffectDt,
                EffectKm = c.EffectKm,
                ExpiryDt = c.ExpiryDt,
                ExpiryKm = c.ExpiryKm,
                LastTransferDt = c.LastTransferDt,
                LastCancelDt = c.LastCancelDt,
                LastReinstateDt = c.LastReinstateDt,
                VehicleKey = c.VehicleKey,
                NumOfKm = c.NumOfKm,
                NumOfMiles = c.NumOfMiles,
                PurchaseDt = c.PurchaseDt,
                DeliveryDt = c.DeliveryDt,
                VehiclePrice = c.VehiclePrice,
                VehicleRebate = c.VehicleRebate,
                LicensePlate = c.LicensePlate,
                StockNum = c.StockNum,
                VehicleCondition = c.VehicleCondition,
                ClassCode = c.ClassCode,
                ImportYN = c.ImportYN,
                ClaimOption = c.ClaimOption,
                CommercialYN = c.CommercialYN,
                CompanyName = c.CompanyName,
                CompanyRepKey = c.CompanyRepKey,
                FinancingType = c.FinancingType,
                FinancedAmt = c.FinancedAmt,
                DownPaymentAmt = c.DownPaymentAmt,
                PromoValue = c.PromoValue,
                APR = c.APR,
                LienHolderId = c.LienHolderId,
                LienHolderLabel = c.LienHolderLabel,
                LienHolderBranchId = c.LienHolderBranchId,
                FinancialInstId = c.FinancialInstId,
                FinancialInstLabel = c.FinancialInstLabel,
                PremiumFinInst = c.PremiumFinInst,
                Customer1Key = c.Customer1Key,
                Customer2Key = c.Customer2Key,
                IsAboriginalYN = c.IsAboriginalYN,
                AboriginalCardNum = c.AboriginalCardNum,
                IsBuyerResidesOnReserveYN = c.IsBuyerResidesOnReserveYN,
                IsBuyerDeliverToReserveYN = c.IsBuyerDeliverToReserveYN,
                Language = c.Language,
                PaymentFreq = c.PaymentFreq,
                PaymentStat = c.PaymentStat,
                PaymentMeth = c.PaymentMeth,
                IsReceivedYN = c.IsReceivedYN,
                FormRevKey = c.FormRevKey,
                ConsentFormRevKey = c.ConsentFormRevKey,
                ContractSource = c.ContractSource,
                ExtContractNum = c.ExtContractNum,
                IsVehicleRegisteredYN = c.IsVehicleRegisteredYN,
                MespMonths = c.MespMonths,
                MespKm = c.MespKm,
                BrokerId = c.BrokerId,
                BrokerName = c.BrokerName,
                ModDtTime = c.ModDtTime,
                ModLoginId = c.ModLoginId,
                ComputedFinanceType = c.ComputedFinanceType,
            }).FirstOrDefaultAsync();
    }
}
