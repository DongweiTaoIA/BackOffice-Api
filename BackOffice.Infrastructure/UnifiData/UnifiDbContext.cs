using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.UnifiData;

public class UnifiDbContext : DbContext
{
    public UnifiDbContext(DbContextOptions<UnifiDbContext> options)
        : base(options)
    {
    }

    public DbSet<DmDealer> Dealers => Set<DmDealer>();
    public DbSet<CfProgram> Programs => Set<CfProgram>();
    public DbSet<CfProduct> Products => Set<CfProduct>();
    public DbSet<EwDealerProgram> EwDealerPrograms => Set<EwDealerProgram>();
    public DbSet<EwDealerProgramMarkup> DealerProgramMarkups => Set<EwDealerProgramMarkup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DmDealer>(entity =>
        {
            entity.ToTable("dmDealer");
            entity.HasKey(e => e.DealerId);
        });

        modelBuilder.Entity<CfProgram>(entity =>
        {
            entity.ToTable("cfProgram");
            entity.HasKey(e => e.ProgramId);
        });

        modelBuilder.Entity<CfProduct>(entity =>
        {
            entity.ToTable("cfProduct");
            entity.HasKey(e => e.ProductId);
        });

        modelBuilder.Entity<EwDealerProgram>(entity =>
        {
            entity.ToTable("EW_cfDealerProgram");
            entity.HasKey(e => e.DealerProgramKey);
            entity.HasIndex(e => new { e.DealerId, e.ProgramId });
        });

        modelBuilder.Entity<EwDealerProgramMarkup>(entity =>
        {
            entity.ToTable("EW_cfDealerProgramMarkup");
            entity.HasKey(e => e.DealerMarkupKey);
            entity.HasIndex(e => new { e.DealerId, e.ProgramId });
        });
    }
}
