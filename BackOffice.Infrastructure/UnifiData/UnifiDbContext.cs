using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.UnifiData;

public class UnifiDbContext : DbContext
{
    public UnifiDbContext(DbContextOptions<UnifiDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
