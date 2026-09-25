using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Patch;

public sealed class PatchAstronomicalImageCommandHandler
    : IRequestHandler<PatchAstronomicalImageCommand>
{
    private readonly IAstronomicalImageRepository _imageRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ICacheStore _cache;

    public PatchAstronomicalImageCommandHandler(
        IAstronomicalImageRepository imageRepository,
        ICatalogUnitOfWork unitOfWork,
        ICacheStore cache)
    {
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(
        PatchAstronomicalImageCommand request,
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

        if (request.Url is not null)
        {
            image.ChangeUrl(request.Url);
        }

        if (request.Urls is not null)
        {
            image.ReplaceUrls(request.Urls);
        }

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