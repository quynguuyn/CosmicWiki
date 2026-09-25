using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(
    Guid Id ) : IRequest;