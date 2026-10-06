using Transfer.Domain.Common;


namespace Transfer.Domain.Transfers;

public sealed class Transfer : AggregateRoot<TransferId>
{

    public AccountId SenderAccountId { get; }
    public AccountId RecipientAccountId { get; }
    public IdempotencyKey IdempotencyKey { get; }
    public long Version { get; private set; } = 0;
    public Money Amount { get; }
    public TransferStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Transfer(
        TransferId transferId,
        AccountId senderAccountId,
        AccountId recipientAccountId,
        IdempotencyKey idempotencyKey,
        Money amount,
        TransferStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        long version,
        string? failureReason)
        : base(transferId)
    {
        if (transferId == default)
            throw new DomainException("TransferId не может быть пустым.");

        if (senderAccountId == default)
            throw new DomainException("Идентификатор счёта отправителя не может быть пустым.");

        if (recipientAccountId == default)
            throw new DomainException("Идентификатор счёта получателя не может быть пустым.");

        if (senderAccountId == recipientAccountId)
            throw new DomainException("Отправитель и получатель не могут быть одинаковыми.");

        if (idempotencyKey == default)
            throw new DomainException("Ключ идемпотентности не может быть пустым.");

        if (amount is null)
            throw new DomainException("Сумма перевода не может быть пустой.");

        if (!Enum.IsDefined(status))
            throw new DomainException("Указан неизвестный статус перевода.");

        if (createdAt == default)
            throw new DomainException("Время создания перевода не указано.");

        if (updatedAt < createdAt)
            throw new DomainException(
                "Время обновления не может быть раньше времени создания.");

        if (version < 0)
            throw new DomainException("Версия перевода не может быть отрицательной.");

        bool requiresFailureReason =
            status is TransferStatus.Failed or TransferStatus.Rejected;

        if (requiresFailureReason && string.IsNullOrWhiteSpace(failureReason))
            throw new DomainException(
                "Для ошибочного или отклонённого перевода должна быть указана причина.");

        if (!requiresFailureReason && failureReason is not null)
            throw new DomainException(
                "Причина ошибки допустима только для ошибочного или отклонённого перевода.");

        SenderAccountId = senderAccountId;
        RecipientAccountId = recipientAccountId;
        IdempotencyKey = idempotencyKey;
        Amount = amount;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Version = version;
        FailureReason = failureReason?.Trim();
    }

    public static Transfer Create(
    TransferId id,
    AccountId senderAccountId,
    AccountId recipientAccountId,
    IdempotencyKey idempotencyKey,
    Money amount,
    DateTimeOffset occurredAt)
    {
        return new Transfer(
            id,
            senderAccountId,
            recipientAccountId,
            idempotencyKey,
            amount,
            TransferStatus.Created,
            occurredAt,
            occurredAt,
            version: 0,
            failureReason: null);
    }

    public static Transfer Restore(
        TransferId id,
        AccountId senderAccountId,
        AccountId recipientAccountId,
        IdempotencyKey idempotencyKey,
        Money amount,
        TransferStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        long version,
        string? failureReason)
    {
        return new Transfer(
            id,
            senderAccountId,
            recipientAccountId,
            idempotencyKey,
            amount,
            status,
            createdAt,
            updatedAt,
            version,
            failureReason);
    }


    public void StartProcessing(DateTimeOffset occurredAt)
    {
        if (Status != TransferStatus.Created)
            throw new DomainException("Начать обработку можно только для созданного перевода.");

        SetStatus(TransferStatus.Processing, occurredAt);
    }

    public void MarkFundsReserved(DateTimeOffset occurredAt)
    {
        if (Status != TransferStatus.Processing)
            throw new DomainException("Зарезервировать средства можно только для обрабатываемого перевода.");

        SetStatus(TransferStatus.FundsReserved, occurredAt);
    }

    public void Succeed(DateTimeOffset occurredAt)
    {
        if (Status != TransferStatus.FundsReserved)
            throw new DomainException("Завершить перевод можно только после резервирования средств.");

        SetStatus(TransferStatus.Succeeded, occurredAt);
    }

    public void Fail(string reason, DateTimeOffset occurredAt)
    {
        if (Status is not (TransferStatus.Processing or TransferStatus.FundsReserved))
            throw new DomainException("Завершить перевод с ошибкой можно только во время обработки или после резервирования средств.");

        SetStatus(TransferStatus.Failed, occurredAt, ValidateReason(reason));
    }

    public void Reject(string reason, DateTimeOffset occurredAt)
    {
        if (Status != TransferStatus.Processing)
            throw new DomainException("Отклонить можно только обрабатываемый перевод.");

        SetStatus(TransferStatus.Rejected, occurredAt, ValidateReason(reason));
    }

    public void Cancel(DateTimeOffset occurredAt)
    {
        if (Status is not (TransferStatus.Created or TransferStatus.Processing))
            throw new DomainException("Отменить перевод можно только до резервирования средств.");

        SetStatus(TransferStatus.Cancelled, occurredAt);
    }

    private void SetStatus(
        TransferStatus status,
        DateTimeOffset occurredAt,
        string? failureReason = null)
    {
        if (occurredAt < CreatedAt || occurredAt < UpdatedAt)
            throw new DomainException("Время изменения статуса не может быть раньше времени создания или предыдущего обновления перевода.");

        Status = status;
        UpdatedAt = occurredAt;
        FailureReason = failureReason;
    }

    private static string ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Причина ошибки или отклонения перевода не может быть пустой.");

        return reason.Trim();
    }
}

