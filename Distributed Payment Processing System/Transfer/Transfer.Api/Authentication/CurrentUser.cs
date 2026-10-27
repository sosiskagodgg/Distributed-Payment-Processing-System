using Transfer.Application.Common;

namespace Transfer.Api.Authentication;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return Guid.Empty;

            var value = user.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}