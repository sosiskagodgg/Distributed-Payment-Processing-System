

namespace Transfer.Application.DTOs
{
    public sealed record TransferDto(
        Guid Id,
        Guid SenderAccountId,
        Guid RecipientAccountId,
        decimal Amount,
        string Currency,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        string? FailureReason)
    {
        public static TransferDto FromTransfer(Domain.Transfers.Transfer transfer)
        {
            
            return new(
                transfer.Id.Value,
                transfer.SenderAccountId.Value,
                transfer.RecipientAccountId.Value,
                transfer.Amount.Amount,
                transfer.Amount.Currency,
                transfer.Status.ToString(),
                transfer.CreatedAt,
                transfer.UpdatedAt,
                transfer.FailureReason);
        }
    }

}
