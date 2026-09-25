using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.PatchCategory;

public sealed record PatchCategoryCommand(
    Guid Id, string? Name, string? Slug, string? Description ) : IRequest;