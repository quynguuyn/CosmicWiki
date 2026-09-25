using FluentValidation;

namespace CosmicWiki.Visit.Application.Features.ChangeVisitRequestStatus;

public sealed class ConfirmVisitRequestValidator : AbstractValidator<ConfirmVisitRequestCommand>
{
    public ConfirmVisitRequestValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeclineVisitRequestValidator : AbstractValidator<DeclineVisitRequestCommand>
{
    public DeclineVisitRequestValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class CancelVisitRequestValidator : AbstractValidator<CancelVisitRequestCommand>
{
    public CancelVisitRequestValidator() => RuleFor(x => x.Id).NotEmpty();
}
