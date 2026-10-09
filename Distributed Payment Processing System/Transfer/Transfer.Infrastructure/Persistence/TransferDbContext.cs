using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TransferEntity = Transfer.Domain.Transfers.Transfer;
namespace Transfer.Infrastructure.Persistence;

public sealed class TransferDbContext : DbContext
{
    public TransferDbContext(
        DbContextOptions<TransferDbContext> options): base(options) { }


    public DbSet<EntityType> Entities => Set<EntityType>();
    public DbSet<TransferEntity> Transfers => Set<TransferEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TransferDbContext).Assembly);
    }
}

