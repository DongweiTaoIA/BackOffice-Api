using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using BackOffice.Infrastructure.UnifiData;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.Services;

public class DealerService(UnifiDbContext unifiDb, IProductRegistry productRegistry) : IDealerService
{
    public async Task<EligibilityResult> CheckEligibilityAsync(string dealerCode, string product)
    {
        var result = new EligibilityResult
        {
            DealerCode = dealerCode.ToUpperInvariant(),
            Product = product,
        };

        var dealer = await unifiDb.Dealers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DealerId == dealerCode);

        if (dealer == null)
        {
            result.IsEligible = false;
            result.DealerName = "Unknown";
            result.Summary = $"Dealer {dealerCode} was not found in the system.";
            return result;
        }

        result.DealerName = dealer.DBAName;

        if (dealer.DealerStat != "A")
        {
            result.IsEligible = false;
            result.Summary = $"Dealer {dealerCode} ({dealer.DBAName}) is currently inactive (status: {dealer.DealerStat}). Cannot sell any products.";
            return result;
        }

        // Resolve product to ProductId code
        var productId = productRegistry.ResolveProduct(product);
        var programs = productId != null
            ? await GetDealerProgramsAsync(dealerCode, productId)
            : [];

        // Check if input refers to a specific program (by code or by description)
        var matchedProgramId = await unifiDb.Programs.AsNoTracking()
            .Where(p => p.ProgramId == product || p.DescnEn == product)
            .Select(p => p.ProgramId)
            .FirstOrDefaultAsync();

        if (matchedProgramId != null)
        {
            programs = programs.Where(p => p.Code.Equals(matchedProgramId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        result.IsEligible = programs.Count > 0;

        if (result.IsEligible)
        {
            result.Programs = programs;
            result.Summary = $"Dealer {dealerCode} ({dealer.DBAName}) CAN sell {product}. {programs.Count} active program(s) available.";
        }
        else
        {
            result.Programs = [];
            result.Summary = $"Dealer {dealerCode} ({dealer.DBAName}) CANNOT sell {product}. This product is not activated for this dealer.";
        }

        return result;
    }

    public async Task<EligibilityResult> CheckEligibilityAsync(EligibilityQuery query)
    {
        DmDealer? dealer = null;
        string dealerCode = string.Empty;

        if (!string.IsNullOrWhiteSpace(query.DealerId))
        {
            dealerCode = query.DealerId.Trim();
            dealer = await unifiDb.Dealers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DealerId == dealerCode);
        }
        else if (!string.IsNullOrWhiteSpace(query.DealerName))
        {
            dealer = await unifiDb.Dealers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DBAName.Contains(query.DealerName.Trim()));
            if (dealer != null)
            {
                dealerCode = dealer.DealerId;
            }
        }

        if (dealer == null)
        {
            return new EligibilityResult
            {
                IsEligible = false,
                DealerCode = dealerCode,
                DealerName = "Unknown",
                Product = query.ProductName ?? query.ProductId ?? "",
                Summary = string.IsNullOrWhiteSpace(dealerCode)
                    ? "No dealer identifier was provided."
                    : $"Dealer {dealerCode} was not found in the system."
            };
        }

        var result = new EligibilityResult
        {
            DealerCode = dealer.DealerId,
            DealerName = dealer.DBAName,
        };

        if (dealer.DealerStat != "A")
        {
            result.IsEligible = false;
            result.Summary = $"Dealer {dealer.DealerId} ({dealer.DBAName}) is currently inactive (status: {dealer.DealerStat}). Cannot sell any products.";
            return result;
        }

        // Resolve product by ID or Name
        string? productId = null;

        if (!string.IsNullOrWhiteSpace(query.ProductId))
        {
            productId = productRegistry.ResolveProduct(query.ProductId.Trim());
        }

        if (productId == null && !string.IsNullOrWhiteSpace(query.ProductName))
        {
            productId = productRegistry.ResolveProduct(query.ProductName.Trim());
        }

        // If program specified, resolve its product
        if (productId == null && !string.IsNullOrWhiteSpace(query.ProgramId))
        {
            productId = await ResolveProgramToProductIdAsync(query.ProgramId.Trim());
        }

        if (productId == null && !string.IsNullOrWhiteSpace(query.ProgramName))
        {
            productId = await ResolveProgramToProductIdAsync(query.ProgramName.Trim());
        }

        if (productId == null)
        {
            // Try to find product from cfProgram using any available query info
            var searchTerm = query.ProductName ?? query.ProductId ?? query.ProgramName ?? query.ProgramId;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productId = await ResolveProgramToProductIdAsync(searchTerm.Trim());
            }
        }

        if (productId == null)
        {
            result.IsEligible = false;
            result.Product = query.ProductName ?? query.ProductId ?? "";
            result.Programs = [];
            result.Summary = $"Dealer {dealer.DealerId} ({dealer.DBAName}) - could not resolve product or program.";
            return result;
        }

        result.Product = query.ProductName ?? query.ProductId ?? productId;

        var programs = await GetDealerProgramsAsync(dealerCode, productId);

        if (programs.Count == 0)
        {
            result.IsEligible = false;
            result.Programs = [];
            result.Summary = $"Dealer {dealer.DealerId} ({dealer.DBAName}) CANNOT sell {result.Product}. This product is not activated for this dealer.";
            return result;
        }

        // Determine if a specific program filter applies:
        // Either from explicit ProgramId/ProgramName, or if ProductId is actually a program code/name
        string? programFilter = null;

        if (!string.IsNullOrWhiteSpace(query.ProgramId) || !string.IsNullOrWhiteSpace(query.ProgramName))
        {
            programFilter = (query.ProgramId ?? query.ProgramName)!.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(query.ProductId))
        {
            var resolvedProgramId = await unifiDb.Programs.AsNoTracking()
                .Where(p => p.ProgramId == query.ProductId.Trim() || p.DescnEn == query.ProductId.Trim())
                .Select(p => p.ProgramId)
                .FirstOrDefaultAsync();

            if (resolvedProgramId != null)
                programFilter = resolvedProgramId;
        }

        if (programFilter != null)
        {
            var matchedProgram = programs.FirstOrDefault(p =>
                p.Code.Equals(programFilter, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(programFilter, StringComparison.OrdinalIgnoreCase));

            if (matchedProgram != null)
            {
                result.IsEligible = true;
                result.Programs = [matchedProgram];
                result.Summary = $"Dealer {dealer.DealerId} ({dealer.DBAName}) CAN sell {result.Product} under program {matchedProgram.Code} ({matchedProgram.Name}).";
            }
            else
            {
                result.IsEligible = false;
                result.Programs = [];
                result.Summary = $"Dealer {dealer.DealerId} ({dealer.DBAName}) CANNOT sell program '{programFilter}'. This program is not activated for this dealer.";
            }
        }
        else
        {
            result.IsEligible = true;
            result.Programs = programs;
            result.Summary = $"Dealer {dealer.DealerId} ({dealer.DBAName}) CAN sell {result.Product}. {programs.Count} active program(s) available.";
        }

        return result;
    }

    public string? GetDealerName(string dealerCode)
    {
        return unifiDb.Dealers
            .AsNoTracking()
            .Where(d => d.DealerId == dealerCode)
            .Select(d => d.DBAName)
            .FirstOrDefault();
    }

    private async Task<List<ProgramInfo>> GetDealerProgramsAsync(string dealerId, string productId)
    {
        var today = DateTime.Today;

        // For EW product, check EW_cfDealerProgram
        if (productId.Equals("EW", StringComparison.OrdinalIgnoreCase))
        {
            return await (
                from dp in unifiDb.EwDealerPrograms.AsNoTracking()
                join p in unifiDb.Programs.AsNoTracking() on dp.ProgramId equals p.ProgramId
                where dp.DealerId == dealerId
                      && dp.EffectDt <= today
                      && dp.ExpiryDt >= today
                orderby p.SortOrder
                select new ProgramInfo
                {
                    Code = p.ProgramId,
                    Name = p.DescnEn,
                    Status = "Active"
                }
            ).ToListAsync();
        }

        // Fallback: return programs from cfProgram by ContractGroup (for products without dealer-program table yet)
        return await unifiDb.Programs
            .AsNoTracking()
            .Where(p => p.ContractGroup == productId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new ProgramInfo
            {
                Code = p.ProgramId,
                Name = p.DescnEn,
                Status = "Active"
            })
            .ToListAsync();
    }

    private async Task<string?> ResolveProgramToProductIdAsync(string programFilter)
    {
        // Find the program in cfProgram by code or description
        var program = await unifiDb.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.ProgramId == programFilter ||
                p.DescnEn.Contains(programFilter));

        if (program == null)
            return null;

        // Determine which product this program belongs to by checking dealer-program tables
        var isEwProgram = await unifiDb.EwDealerPrograms
            .AsNoTracking()
            .AnyAsync(dp => dp.ProgramId == program.ProgramId);

        if (isEwProgram)
            return "EW";

        // Fallback: return null (extend with other product tables as they are added)
        return null;
    }

    public async Task<ProgramActionResult> ActivateProgramAsync(ActivateRequest request)
    {
        var dealer = await unifiDb.Dealers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DealerId == request.DealerId);

        if (dealer == null)
            return new ProgramActionResult { Success = false, Summary = $"Dealer {request.DealerId} was not found." };

        var program = await unifiDb.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId);

        if (program == null)
            return new ProgramActionResult { Success = false, Summary = $"Program {request.ProgramId} was not found." };

        var effectiveDate = !string.IsNullOrWhiteSpace(request.EffectiveDate)
            ? DateTime.Parse(request.EffectiveDate)
            : DateTime.Today;

        // Check if already active
        var existing = await unifiDb.EwDealerPrograms
            .FirstOrDefaultAsync(dp => dp.DealerId == request.DealerId
                                       && dp.ProgramId == request.ProgramId
                                       && dp.ExpiryDt >= DateTime.Today);

        if (existing != null)
        {
            return new ProgramActionResult
            {
                Success = false,
                DealerCode = dealer.DealerId,
                DealerName = dealer.DBAName,
                ProgramId = program.ProgramId,
                ProgramName = program.DescnEn,
                Action = "activate",
                Summary = $"Program {program.ProgramId} ({program.DescnEn}) is already active for dealer {dealer.DealerId} ({dealer.DBAName})."
            };
        }

        // Create new enrollment
        var newEntry = new EwDealerProgram
        {
            DealerId = request.DealerId,
            ProgramId = request.ProgramId,
            DlrPortalYN = "Y",
            AdminDBYN = "Y",
            EffectDt = effectiveDate,
            ExpiryDt = new DateTime(2099, 12, 31),
        };

        unifiDb.EwDealerPrograms.Add(newEntry);
        await unifiDb.SaveChangesAsync();

        return new ProgramActionResult
        {
            Success = true,
            DealerCode = dealer.DealerId,
            DealerName = dealer.DBAName,
            ProgramId = program.ProgramId,
            ProgramName = program.DescnEn,
            Product = program.ContractGroup,
            Action = "activate",
            EffectiveDate = effectiveDate.ToString("yyyy-MM-dd"),
            Summary = $"Program {program.ProgramId} ({program.DescnEn}) has been activated for dealer {dealer.DealerId} ({dealer.DBAName}) effective {effectiveDate:yyyy-MM-dd}."
        };
    }

    public async Task<ProgramActionResult> DeactivateProgramAsync(DeactivateRequest request)
    {
        var dealer = await unifiDb.Dealers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DealerId == request.DealerId);

        if (dealer == null)
            return new ProgramActionResult { Success = false, Summary = $"Dealer {request.DealerId} was not found." };

        var program = await unifiDb.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId);

        if (program == null)
            return new ProgramActionResult { Success = false, Summary = $"Program {request.ProgramId} was not found." };

        var expiryDate = DateTime.Parse(request.ExpiryDate);

        // Find active enrollment
        var existing = await unifiDb.EwDealerPrograms
            .FirstOrDefaultAsync(dp => dp.DealerId == request.DealerId
                                       && dp.ProgramId == request.ProgramId
                                       && dp.ExpiryDt >= DateTime.Today);

        if (existing == null)
        {
            return new ProgramActionResult
            {
                Success = false,
                DealerCode = dealer.DealerId,
                DealerName = dealer.DBAName,
                ProgramId = program.ProgramId,
                ProgramName = program.DescnEn,
                Action = "deactivate",
                Summary = $"Program {program.ProgramId} ({program.DescnEn}) is not currently active for dealer {dealer.DealerId} ({dealer.DBAName})."
            };
        }

        // Set expiry date
        existing.ExpiryDt = expiryDate;
        await unifiDb.SaveChangesAsync();

        return new ProgramActionResult
        {
            Success = true,
            DealerCode = dealer.DealerId,
            DealerName = dealer.DBAName,
            ProgramId = program.ProgramId,
            ProgramName = program.DescnEn,
            Product = program.ContractGroup,
            Action = "deactivate",
            EffectiveDate = expiryDate.ToString("yyyy-MM-dd"),
            Summary = $"Program {program.ProgramId} ({program.DescnEn}) for dealer {dealer.DealerId} ({dealer.DBAName}) will be deactivated effective {expiryDate:yyyy-MM-dd}."
        };
    }

    public async Task<MaxMarkupResult> GetMaxMarkupAsync(string programIdOrName)
    {
        // Resolve program by ID or name
        var program = await unifiDb.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProgramId == programIdOrName
                                      || p.DescnEn.Contains(programIdOrName));

        if (program == null)
        {
            return new MaxMarkupResult
            {
                Found = false,
                Summary = $"Program '{programIdOrName}' was not found."
            };
        }

        var maxMarkup = await unifiDb.DealerProgramMarkups
            .AsNoTracking()
            .Where(m => m.ProgramId == program.ProgramId)
            .Select(m => (decimal?)m.MarkupVal)
            .MaxAsync();

        if (maxMarkup == null)
        {
            return new MaxMarkupResult
            {
                Found = true,
                ProgramId = program.ProgramId,
                ProgramName = program.DescnEn,
                MaxMarkup = 0,
                Summary = $"No markup records found for program {program.ProgramId} ({program.DescnEn})."
            };
        }

        return new MaxMarkupResult
        {
            Found = true,
            ProgramId = program.ProgramId,
            ProgramName = program.DescnEn,
            MaxMarkup = maxMarkup.Value,
            Summary = $"The maximum markup for program {program.ProgramId} ({program.DescnEn}) is {maxMarkup.Value:F2}."
        };
    }
}
