
using Microsoft.EntityFrameworkCore;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Application.DTOs;
using Transfer.Domain.Transfers;

namespace Transfer.Infrastructure.Persistence.Queries;

public class TransferQueries : ITransferQueries
{
    private readonly TransferDbContext _transferDbContext;

    public TransferQueries(TransferDbContext transferDbContext)
    {
        _transferDbContext = transferDbContext;
    }

    public async Task<TransferDto?> GetByIdAsync(TransferId id, CancellationToken cancellationToken)
    {
        var transfer = await _transferDbContext.Transfers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (transfer == null) return null;

        return TransferDto.FromTransfer(transfer); 

    }
}

