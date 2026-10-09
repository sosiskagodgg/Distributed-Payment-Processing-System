
using Microsoft.EntityFrameworkCore;
using Transfer.Domain.Transfers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;


namespace Transfer.Infrastructure.Persistence.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<TransferAggregate>
{
    public void Configure(EntityTypeBuilder<TransferAggregate> builder)
    {
        builder.ToTable("Transfers");

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => new
        {
            t.SenderAccountId,
            t.IdempotencyKey
        }).IsUnique();
        
        builder.Property(t => t.Id)
            .HasConversion(
                transferId => transferId.Value,
                value => new TransferId(value)
            ).ValueGeneratedNever();

        builder.Property(t=>t.SenderAccountId)
            .HasConversion(
                id => id.Value,
                value => new AccountId(value)
            ).IsRequired();

        builder.Property(t => t.RecipientAccountId)
            .HasConversion(
                id => id.Value,
                value => new AccountId(value)
            ).IsRequired();

        builder.Property(t => t.IdempotencyKey)
            .HasConversion(
                key => key.Value,
                value => new IdempotencyKey(value)
            ).IsRequired();

        builder.Property(t => t.Version).IsRequired().IsConcurrencyToken();

        builder.Property(t => t.Status).HasConversion(
            status => status.ToString(),
            value => Enum.Parse<TransferStatus>(value)
            ).HasMaxLength(32).IsRequired(); 

        builder.ComplexProperty(t=>t.Amount, amountBuilder =>
        {
            amountBuilder.Property(a => a.Amount).HasPrecision(18, 2).HasColumnName("Amount").IsRequired();

            amountBuilder.Property(a => a.Currency).HasMaxLength(3).HasColumnName("Currency").IsRequired();
        });

        builder.Property(t=> t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        builder.Property(t => t.FailureReason).HasMaxLength(500);
    }
}

