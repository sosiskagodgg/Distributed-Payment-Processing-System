using MediatR;
using Transfer.Application.Common;
using Transfer.Application.Transfers.Commands.CreateTransfer;
namespace Transfer.Api;

public static class TransferEndpoints
{
    public static IEndpointRouteBuilder MapTransferEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/transfers", async (CreateTransferCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);


            if (result.IsSuccess)
            {
                var id = result.Value.Value;
                return Results.Created($"/transfers/{id}", new { Id = id });
            }

            return result.ErrorType switch
            {
                ErrorType.Validation => Results.BadRequest(new { error = result.Error }),
                ErrorType.Conflict => Results.Conflict(new { error = result.Error }),
                _ => Results.Problem()
            };
        }).RequireAuthorization(); 
        return endpoints;

    }
}
