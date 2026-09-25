using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id, string Name, string Slug, string? Description ) : IRequest;