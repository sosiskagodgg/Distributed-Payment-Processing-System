using FluentValidation;
namespace Transfer.Application.Transfers.Commands.CreateTransfer;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.SenderAccountId).NotEmpty().WithMessage("Аккаунт отправителя не может быть пустым.");
        
        RuleFor(x => x.RecipientAccountId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Аккаунт получателя не может быть пустым.")
            .NotEqual(x => x.SenderAccountId)
            .WithMessage("Аккаунты не могут совпадать.");
       
        RuleFor(x=>x.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым.");
       
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма перевода должна быть положительной.");
        
        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$").WithMessage("Неккоректный формат валюты");


    }
}
