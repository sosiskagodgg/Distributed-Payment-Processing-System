using FluentValidation;
namespace Transfer.Application.Transfers.Queries.GetTransferById;

public class GetTransferByIdQueryValidator : AbstractValidator<GetTransferByIdQuery>
{
    public GetTransferByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Айди перевода не может быть пустым.");
    }
}
