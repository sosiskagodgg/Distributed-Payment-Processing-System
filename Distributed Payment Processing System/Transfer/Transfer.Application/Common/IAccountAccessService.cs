
namespace Transfer.Application.Common;

public interface IAccountAccessService
{
    Task<bool> CanAccessAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
}
