using System.Data;
using System.Data.Common;
using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using BackOffice.Infrastructure.UnifiData;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class ContractService(UnifiDbContext unifiDb) : IContractService
{
    public async Task<ContractPagedResult> GetContractsPagedAsync(int page, int pageSize, string? search, string? status, string? product, string? dealerId, string? sortBy, string? sortDir)
    {
        var dbQuery =
            from c in unifiDb.Contracts.AsNoTracking()
            join d in unifiDb.Dealers.AsNoTracking() on c.DealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            select new { Contract = c, DealerName = d != null ? d.DBAName : null };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.Like(x.Contract.ContractNum, pattern)
                || EF.Functions.Like(x.Contract.DealerId, pattern)
                || (x.Contract.ExtContractNum != null && EF.Functions.Like(x.Contract.ExtContractNum, pattern))
                || (x.DealerName != null && EF.Functions.Like(x.DealerName, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            dbQuery = dbQuery.Where(x => x.Contract.ContractStatPri == status.Trim());

        if (!string.IsNullOrWhiteSpace(product))
            dbQuery = dbQuery.Where(x => x.Contract.ProductId == product.Trim());

        if (!string.IsNullOrWhiteSpace(dealerId))
        {
            var pattern = $"%{dealerId.Trim()}%";
            dbQuery = dbQuery.Where(x => EF.Functions.Like(x.Contract.DealerId, pattern));
        }

        var totalCount = await dbQuery.CountAsync();

        // Sorting
        var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        dbQuery = (sortBy?.ToLower()) switch
        {
            "contractnum" => isDesc ? dbQuery.OrderByDescending(x => x.Contract.ContractNum) : dbQuery.OrderBy(x => x.Contract.ContractNum),
            "dealerid" => isDesc ? dbQuery.OrderByDescending(x => x.Contract.DealerId) : dbQuery.OrderBy(x => x.Contract.DealerId),
            "productid" => isDesc ? dbQuery.OrderByDescending(x => x.Contract.ProductId) : dbQuery.OrderBy(x => x.Contract.ProductId),
            "status" => isDesc ? dbQuery.OrderByDescending(x => x.Contract.ContractStatPri) : dbQuery.OrderBy(x => x.Contract.ContractStatPri),
            "effectdt" => isDesc ? dbQuery.OrderByDescending(x => x.Contract.EffectDt) : dbQuery.OrderBy(x => x.Contract.EffectDt),
            "expirydt" => isDesc ? dbQuery.OrderByDescending(x => x.Contract.ExpiryDt) : dbQuery.OrderBy(x => x.Contract.ExpiryDt),
            _ => dbQuery.OrderByDescending(x => x.Contract.EffectDt),
        };

        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new ContractPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            HasMore = page * pageSize < totalCount,
        };
    }

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
        var dto = await (
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

        if (dto != null)
        {
            await EnrichContractDetailsAsync(dto);
        }

        return dto;
    }

    /// <summary>
    /// Populates customer / vehicle / claim info that lives in tables not currently
    /// mapped as EF entities (dmCustomer, dmVehicle, dmClaim) using raw ADO.NET.
    /// </summary>
    private async Task EnrichContractDetailsAsync(ContractDetailsDto dto)
    {
        var connection = unifiDb.Database.GetDbConnection();
        var openedHere = false;
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            openedHere = true;
        }

        try
        {
            // Primary customer (+ address / contact)
            if (dto.Customer1Key.HasValue)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT FirstName, LastName, AddrLine1, AddrLine2, City, ProvState, PostalZip, PhoneNum1, Email FROM dmCustomer WHERE CustomerKey = @k";
                cmd.Parameters.Add(new SqlParameter("@k", dto.Customer1Key.Value));
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    dto.Customer1Name = JoinNonEmpty(" ",
                        SafeString(r, "FirstName").Trim(),
                        SafeString(r, "LastName").Trim());

                    dto.CustomerAddress = JoinNonEmpty(", ",
                        SafeString(r, "AddrLine1").Trim(),
                        SafeString(r, "AddrLine2").Trim());

                    dto.CustomerCity = NullIfBlank(SafeString(r, "City").Trim());
                    dto.CustomerProvState = NullIfBlank(SafeString(r, "ProvState").Trim());
                    dto.CustomerPostalZip = NullIfBlank(SafeString(r, "PostalZip").Trim());
                    dto.CustomerPhone = NullIfBlank(SafeString(r, "PhoneNum1").Trim());
                    dto.CustomerEmail = NullIfBlank(SafeString(r, "Email").Trim());
                }
            }

            // Co-applicant (secondary customer) — name only
            if (dto.Customer2Key.HasValue)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT FirstName, LastName FROM dmCustomer WHERE CustomerKey = @k";
                cmd.Parameters.Add(new SqlParameter("@k", dto.Customer2Key.Value));
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    dto.Customer2Name = JoinNonEmpty(" ",
                        SafeString(r, "FirstName").Trim(),
                        SafeString(r, "LastName").Trim());
                }
            }

            // Vehicle
            if (dto.VehicleKey.HasValue)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT VIN, Year, Make, Model, Odometer FROM dmVehicle WHERE VehicleKey = @k";
                cmd.Parameters.Add(new SqlParameter("@k", dto.VehicleKey.Value));
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    dto.Vin = NullIfBlank(SafeString(r, "VIN").Trim());
                    var yearOrd = SafeOrdinal(r, "Year");
                    if (yearOrd >= 0 && !r.IsDBNull(yearOrd))
                        dto.VehicleYear = Convert.ToInt16(r.GetValue(yearOrd));
                    dto.VehicleMake = NullIfBlank(SafeString(r, "Make").Trim());
                    dto.VehicleModel = NullIfBlank(SafeString(r, "Model").Trim());
                    var odoOrd = SafeOrdinal(r, "Odometer");
                    if (odoOrd >= 0 && !r.IsDBNull(odoOrd))
                        dto.VehicleOdometer = Convert.ToInt32(r.GetValue(odoOrd));
                }
            }

            // Claim counts (open + total) — "open" = primary status not Closed/Cancelled/Void/Denied
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT
                        SUM(CASE WHEN ClaimStatPri NOT IN ('C','X','V','D') THEN 1 ELSE 0 END) AS OpenCount,
                        COUNT(*) AS TotalCount
                    FROM dmClaim
                    WHERE ContractKey = @k";
                cmd.Parameters.Add(new SqlParameter("@k", dto.ContractKey));
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    var openOrd = SafeOrdinal(r, "OpenCount");
                    if (openOrd >= 0 && !r.IsDBNull(openOrd))
                        dto.OpenClaimCount = Convert.ToInt32(r.GetValue(openOrd));
                    var totalOrd = SafeOrdinal(r, "TotalCount");
                    if (totalOrd >= 0 && !r.IsDBNull(totalOrd))
                        dto.TotalClaimCount = Convert.ToInt32(r.GetValue(totalOrd));
                }
            }
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string? JoinNonEmpty(string separator, params string?[] parts)
    {
        var kept = parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()).ToArray();
        return kept.Length == 0 ? null : string.Join(separator, kept);
    }

    public async Task<ContractStatusDto?> GetContractStatusAsync(string contractNum)
    {
        if (string.IsNullOrWhiteSpace(contractNum))
            return null;

        var trimmed = contractNum.Trim();

        var row = await (
            from c in unifiDb.Contracts.AsNoTracking()
            join d in unifiDb.Dealers.AsNoTracking() on c.DealerId equals d.DealerId into dj
            from d in dj.DefaultIfEmpty()
            where c.ContractNum == trimmed
            select new
            {
                c.ContractNum,
                c.ContractStatPri,
                c.ProductId,
                c.DealerId,
                DealerName = d != null ? d.DBAName : null,
                c.EffectDt,
                c.ModDtTime,
            }).FirstOrDefaultAsync();

        if (row == null)
            return null;

        return new ContractStatusDto
        {
            ContractId = row.ContractNum.Trim(),
            Status = MapContractStatus(row.ContractStatPri),
            Owner = string.IsNullOrWhiteSpace(row.DealerName) ? row.DealerId : row.DealerName!,
            Product = row.ProductId,
            EffectiveDate = row.EffectDt.ToString("yyyy-MM-dd"),
            LastUpdated = row.ModDtTime,
            Source = "dmContract",
        };
    }

    private static string MapContractStatus(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Unknown";

        return code.Trim().ToUpperInvariant() switch
        {
            "A" => "Active",
            "C" => "Cancelled",
            "E" => "Expired",
            "P" => "Pending",
            "F" => "Finalized",
            "I" => "Inactive",
            _ => code.Trim(),
        };
    }

    public async Task<CancellationEligibilityResult?> CheckCancellationEligibilityAsync(
        string contractNum,
        DateTime cancellationDate,
        string? ruleId,
        string? cancType,
        string? lang,
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(contractNum))
            return null;

        var trimmed = contractNum.Trim();

        var contract = await unifiDb.Contracts.AsNoTracking()
            .Where(c => c.ContractNum == trimmed)
            .Select(c => new
            {
                c.ContractKey,
                c.ContractNum,
                c.ProductId,
                c.ContractStatPri,
                c.ContractStatSec,
                c.EffectDt,
                c.ExpiryDt,
            })
            .FirstOrDefaultAsync();

        if (contract == null)
            return null;

        var pri = (contract.ContractStatPri ?? string.Empty).Trim();
        var sec = (contract.ContractStatSec ?? string.Empty).Trim();

        var result = new CancellationEligibilityResult
        {
            ContractNumber = contract.ContractNum.Trim(),
            Status = $"{pri}/{sec}",
            Product = contract.ProductId,
            EffectiveDate = contract.EffectDt.ToString("yyyy-MM-dd"),
            ExpiryDate = contract.ExpiryDt?.ToString("yyyy-MM-dd") ?? string.Empty,
        };

        // Gate: primary status must be A (Active) and secondary status must be F (Finalized)
        if (!string.Equals(pri, "A", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(sec, "F", StringComparison.OrdinalIgnoreCase))
        {
            result.IsEligible = false;
            result.Reason = $"Contract status must be Active (A) and Finalized (F) to be cancellable. Current status: {pri}/{sec}.";
            result.Summary = $"Contract {result.ContractNumber} is NOT eligible for cancellation. {result.Reason}";
            return result;
        }

        // Call SP DPP_DP612ContractCancel_Calc to compute the refund
        var (messages, refund) = await CalculateCancellationAsync(
            contract.ContractKey,
            cancellationDate,
            string.IsNullOrWhiteSpace(ruleId) ? "CR01H" : ruleId.Trim(),
            string.IsNullOrWhiteSpace(cancType) ? "CS" : cancType.Trim(),
            string.IsNullOrWhiteSpace(lang) ? "E" : lang.Trim(),
            string.IsNullOrWhiteSpace(userId) ? "SXR" : userId.Trim());

        result.Messages = messages;
        result.RefundDetails = refund;
        result.RefundAmount = refund?.TotalRefund ?? refund?.NetRefundAmount ?? refund?.RefundAmount;
        result.RefundType = DetermineRefundType(refund);

        // Any message returned by the calc SP means the contract cannot be cancelled.
        var combinedMessage = messages.Count > 0
            ? string.Join(" ", messages.Select(m => m.MsgText).Where(t => !string.IsNullOrWhiteSpace(t)))
            : string.Empty;

        if (messages.Count > 0)
        {
            result.IsEligible = false;
            result.Reason = combinedMessage;
            result.Summary = string.IsNullOrWhiteSpace(combinedMessage)
                ? $"Contract {result.ContractNumber} is NOT eligible for cancellation."
                : $"Contract {result.ContractNumber} is NOT eligible for cancellation. {combinedMessage}";
        }
        else
        {
            result.IsEligible = true;
            result.Reason = string.Empty;
            result.Summary = result.RefundAmount.HasValue
                ? $"Contract {result.ContractNumber} cancellation calculated for {cancellationDate:yyyy-MM-dd}. Refund: {result.RefundAmount:C}."
                : $"Contract {result.ContractNumber} cancellation calculated for {cancellationDate:yyyy-MM-dd}. No refund amount was returned by the calculator.";
        }

        return result;
    }

    private async Task<(List<CancellationMessage> Messages, CancellationRefundDetails? Refund)> CalculateCancellationAsync(
        int contractKey,
        DateTime cancellationDate,
        string ruleId,
        string cancType,
        string lang,
        string userId)
    {
        var connection = unifiDb.Database.GetDbConnection();
        var openedHere = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            openedHere = true;
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DPP_DP612ContractCancel_Calc";

            command.Parameters.Add(new SqlParameter("@piContractKey", SqlDbType.Int) { Value = contractKey });
            command.Parameters.Add(new SqlParameter("@pdCancDt", SqlDbType.DateTime) { Value = cancellationDate });
            command.Parameters.Add(new SqlParameter("@psRuleId", SqlDbType.VarChar, 10) { Value = ruleId });
            command.Parameters.Add(new SqlParameter("@psCancType", SqlDbType.VarChar, 2) { Value = cancType });
            command.Parameters.Add(new SqlParameter("@psLang", SqlDbType.Char, 1) { Value = lang });
            command.Parameters.Add(new SqlParameter("@psUserId", SqlDbType.VarChar, 50) { Value = userId });

            var messages = new List<CancellationMessage>();
            CancellationRefundDetails? refund = null;

            await using var reader = await command.ExecuteReaderAsync();

            // First result set: messages
            while (await reader.ReadAsync())
            {
                messages.Add(new CancellationMessage
                {
                    MsgText = SafeString(reader, "MsgText"),
                    MsgType = SafeString(reader, "MsgType"),
                });
            }

            // Second result set: refund detail row
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                refund = new CancellationRefundDetails
                {
                    RetailPremiumPaidAmount = SafeDecimal(reader, "RetailPremiumPaidAmount"),
                    RefundAmount = SafeDecimal(reader, "RefundAmount"),
                    Factor = SafeDecimal(reader, "Factor") ?? SafeDecimal(reader, "RefundFactor"),
                    ClaimsPaidAmount = SafeDecimal(reader, "ClaimsPaidAmount"),
                    AdminFee = SafeDecimal(reader, "AdminFee"),
                    NetRefundAmount = SafeDecimal(reader, "NetRefundAmount"),
                    RefundTax1Amount = SafeDecimal(reader, "RefundTax1Amount"),
                    RefundTax2Amount = SafeDecimal(reader, "RefundTax2Amount"),

                    // Dealer chargeback columns from the same row.
                    DealerMarkupAmount = SafeDecimal(reader, "DealerMarkupAmount"),
                    DealerMarkupPercentage = SafeDecimal(reader, "DealerMarkupPercentage"),
                    NetDealerChargebackAmount = SafeDecimal(reader, "NetDealerChargebackAmount"),
                    ChargebackTax1Amount = SafeDecimal(reader, "ChargebackTax1Amount"),
                    ChargebackTax2Amount = SafeDecimal(reader, "ChargebackTax2Amount"),
                    DealerChargebackAmount = SafeDecimal(reader, "DealerChargebackAmount"),
                    IapPortionAmount = SafeDecimal(reader, "IapPortionAmount"),

                    // Tax & cheque flags.
                    Tax1Value = SafeDecimal(reader, "Tax1Value"),
                    Tax2Value = SafeDecimal(reader, "Tax2Value"),
                    EnableIssueChequeYN = SafeString(reader, "EnableIssueChequeYN"),
                    Tax1RemitYN = SafeString(reader, "Tax1RemitYN"),
                    Tax2RemitYN = SafeString(reader, "Tax2RemitYN"),
                };

                // Total Refund is not returned by the SP; compute it the same way the
                // desktop Customer Refund panel does: Net Refund + GST/HST + PST/IPT.
                if (refund.NetRefundAmount.HasValue)
                {
                    refund.TotalRefund = refund.NetRefundAmount.Value
                        + (refund.RefundTax1Amount ?? 0m)
                        + (refund.RefundTax2Amount ?? 0m);
                }
            }

            return (messages, refund);
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    private static string DetermineRefundType(CancellationRefundDetails? details)
    {
        if (details == null)
            return string.Empty;

        var refund = details.TotalRefund ?? details.NetRefundAmount ?? details.RefundAmount;
        var paid = details.RetailPremiumPaidAmount;

        if (refund is null)
            return string.Empty;
        if (refund == 0m)
            return "No Refund";
        if (paid.HasValue && refund >= paid.Value)
            return "Full Refund";

        return "Pro-Rated";
    }

    private static int SafeOrdinal(DbDataReader reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static string SafeString(DbDataReader reader, string columnName)
    {
        var ord = SafeOrdinal(reader, columnName);
        if (ord < 0 || reader.IsDBNull(ord))
            return string.Empty;
        return reader.GetValue(ord)?.ToString() ?? string.Empty;
    }

    private static decimal? SafeDecimal(DbDataReader reader, string columnName)
    {
        var ord = SafeOrdinal(reader, columnName);
        if (ord < 0 || reader.IsDBNull(ord))
            return null;
        return Convert.ToDecimal(reader.GetValue(ord));
    }
}
