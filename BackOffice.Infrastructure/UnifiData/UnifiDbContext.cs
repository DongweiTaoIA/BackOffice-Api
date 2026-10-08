using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.UnifiData;

public class UnifiDbContext : DbContext
{
    public UnifiDbContext(DbContextOptions<UnifiDbContext> options)
        : base(options)
    {
    }

    public DbSet<DmDealer> Dealers => Set<DmDealer>();
    public DbSet<DmContract> Contracts => Set<DmContract>();
    public DbSet<DmClaim> Claims => Set<DmClaim>();
    public DbSet<CfProgram> Programs => Set<CfProgram>();
    public DbSet<CfProduct> Products => Set<CfProduct>();
    public DbSet<CfContractGroup> ContractGroups => Set<CfContractGroup>();
    public DbSet<EwDealerProgram> EwDealerPrograms => Set<EwDealerProgram>();
    public DbSet<EwDealerProgramMarkup> DealerProgramMarkups => Set<EwDealerProgramMarkup>();
    public DbSet<DwDmContract> DwContracts => Set<DwDmContract>();
    public DbSet<GpDmContract> GpContracts => Set<GpDmContract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DmDealer>(entity =>
        {
            entity.ToTable("dmDealer");
            entity.HasKey(e => e.DealerId);
        });

        modelBuilder.Entity<DmContract>(entity =>
        {
            entity.ToTable("dmContract");
            entity.HasKey(e => e.ContractKey);
            entity.HasIndex(e => e.ContractNum);
            entity.HasIndex(e => e.DealerId);
            entity.Property(e => e.ComputedFinanceType).ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<DmClaim>(entity =>
        {
            entity.ToTable("dmClaim");
            entity.HasKey(e => e.ClaimKey);
            entity.HasIndex(e => e.ClaimNum).IsUnique();
            entity.HasIndex(e => e.ContractKey);
            entity.HasIndex(e => e.RONum);
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

        modelBuilder.Entity<CfContractGroup>(entity =>
        {
            entity.ToTable("cfContractGroup");
            entity.HasKey(e => e.ContractGroup);
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

        modelBuilder.Entity<DwDmContract>(entity =>
        {
            entity.ToTable("DW_dmContract");
            entity.HasKey(e => e.ContractKey);
        });

        modelBuilder.Entity<GpDmContract>(entity =>
        {
            entity.ToTable("GP_dmContract");
            entity.HasKey(e => e.ContractKey);
        });
    }
}
