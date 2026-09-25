using FluentValidation;
namespace CosmicWiki.Visit.Application.Features.CreateVisitRequest;
public sealed class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequestCommand>
{
    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.RequestedDateUtc).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
        RuleFor(x => x.PartySize).InclusiveBetween(1, 20);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
