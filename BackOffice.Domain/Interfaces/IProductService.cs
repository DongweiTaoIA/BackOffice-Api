using BackOffice.Domain.Models;

namespace BackOffice.Domain.Interfaces;

public interface IProductService
{
    Task<List<ProgramLookupResult>> SearchProgramsAsync(string query);
}
