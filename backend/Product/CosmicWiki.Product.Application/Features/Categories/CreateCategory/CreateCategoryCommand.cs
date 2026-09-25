using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description
) : IRequest<Guid>;