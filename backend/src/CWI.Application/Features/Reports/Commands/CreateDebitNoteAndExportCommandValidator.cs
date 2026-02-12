using FluentValidation;

namespace CWI.Application.Features.Reports.Commands;

/// <summary>
/// Debit note oluşturma isteği doğrulayıcısı.
/// </summary>
public class CreateDebitNoteAndExportCommandValidator : AbstractValidator<CreateDebitNoteAndExportCommand>
{
    public CreateDebitNoteAndExportCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("İstek boş olamaz.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.CustomerCode)
                .NotEmpty()
                .WithMessage("Müşteri kodu zorunludur.");

            RuleFor(x => x.Request.OrderId)
                .GreaterThan(0)
                .WithMessage("Sipariş id sıfırdan büyük olmalıdır.");

            RuleFor(x => x.Request.InvoiceNo)
                .NotEmpty()
                .WithMessage("Invoice no zorunludur.");

            RuleFor(x => x.Request.Amount)
                .GreaterThan(0)
                .WithMessage("Tutar sıfırdan büyük olmalıdır.");

            RuleFor(x => x.Request.DebitNoteDate)
                .NotEmpty()
                .WithMessage("Debit note tarihi zorunludur.");
        });
    }
}
