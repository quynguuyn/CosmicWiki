using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Delete;

public sealed class DeleteAstronomicalImageCommandHandler
    : IRequestHandler<DeleteAstronomicalImageCommand>
{
    private readonly IAstronomicalImageRepository _imageRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ICacheStore _cache;

    public DeleteAstronomicalImageCommandHandler(
        IAstronomicalImageRepository imageRepository,
        ICatalogUnitOfWork unitOfWork,
        ICacheStore cache)
    {
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(
        DeleteAstronomicalImageCommand request,
        CancellationToken cancellationToken)
    {
        var image = await _imageRepository.GetByIdAsync(
            request.AstronomicalObjectId,
            request.ImageId,
            cancellationToken);

        if (image is null)
        {
            throw new KeyNotFoundException(
                $"Image '{request.ImageId}' was not found.");
        }

        _imageRepository.Delete(image);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _cache.RemoveAsync(
            $"image:{request.ImageId:N}",
            cancellationToken);
        await _cache.RemoveAsync(
            $"star:{request.AstronomicalObjectId:N}:images",
            cancellationToken);
    }
}