using CosmicWiki.Visit.Application.Features.Events;
using CosmicWiki.Visit.Application.Features.Exhibitions;
using CosmicWiki.Visit.Application.Features.Faqs;
using FluentValidation;

namespace CosmicWiki.Visit.Application.Features.Exhibitions
{

public sealed class CreateExhibitionValidator : AbstractValidator<CreateExhibitionCommand>
{
    public CreateExhibitionValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ImageUrl).MaximumLength(2000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateExhibitionValidator : AbstractValidator<UpdateExhibitionCommand>
{
    public UpdateExhibitionValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ImageUrl).MaximumLength(2000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
}

namespace CosmicWiki.Visit.Application.Features.Events
{

public sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Specification).MaximumLength(500);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StatusType).MaximumLength(50);
        RuleFor(x => x.Action).MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateEventValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Specification).MaximumLength(500);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StatusType).MaximumLength(50);
        RuleFor(x => x.Action).MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
}

namespace CosmicWiki.Visit.Application.Features.Faqs
{

public sealed class CreateFaqValidator : AbstractValidator<CreateFaqCommand>
{
    public CreateFaqValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateFaqValidator : AbstractValidator<UpdateFaqCommand>
{
    public UpdateFaqValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
    }
}
