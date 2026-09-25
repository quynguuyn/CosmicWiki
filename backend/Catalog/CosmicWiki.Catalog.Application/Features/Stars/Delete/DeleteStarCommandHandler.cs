using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Delete;

public sealed class DeleteStarCommandHandler
    : IRequestHandler<DeleteStarCommand>
{
    private readonly IStarRepository _starRepository;
    private readonly IAstronomicalObjectRepository _astronomicalObjectRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IStarCache _starCache;

    public DeleteStarCommandHandler(
        IStarRepository starRepository,
        IAstronomicalObjectRepository astronomicalObjectRepository,
        ICatalogUnitOfWork unitOfWork,
        IStarCache starCache)
    {
        _starRepository = starRepository;
        _astronomicalObjectRepository = astronomicalObjectRepository;
        _unitOfWork = unitOfWork;
        _starCache = starCache;
    }

    public async Task Handle(
        DeleteStarCommand request,
        CancellationToken cancellationToken)
    {
        var star = await _starRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (star is null)
        {
            throw new KeyNotFoundException(
                $"Star '{request.Id}' was not found.");
        }

        var astronomicalObject =
            await _astronomicalObjectRepository.GetByIdAsync(
                star.Id,
                cancellationToken);

        if (astronomicalObject is null)
        {
            throw new InvalidOperationException(
                $"AstronomicalObject for star '{request.Id}' was not found.");
        }

        _astronomicalObjectRepository.Delete(
            astronomicalObject);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _starCache.InvalidateAsync(request.Id, cancellationToken);
    }
}