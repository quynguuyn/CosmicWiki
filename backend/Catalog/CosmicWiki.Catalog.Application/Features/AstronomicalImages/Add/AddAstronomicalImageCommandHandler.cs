using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Add;

public sealed class AddAstronomicalImageCommandHandler
    : IRequestHandler<AddAstronomicalImageCommand, Guid>
{
    private readonly IAstronomicalObjectRepository
        _astronomicalObjectRepository;

    private readonly IAstronomicalImageRepository
        _astronomicalImageRepository;

    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ICacheStore _cache;

    public AddAstronomicalImageCommandHandler(
        IAstronomicalObjectRepository astronomicalObjectRepository,
        IAstronomicalImageRepository astronomicalImageRepository,
        ICatalogUnitOfWork unitOfWork,
        ICacheStore cache)
    {
        _astronomicalObjectRepository =
            astronomicalObjectRepository;

        _astronomicalImageRepository =
            astronomicalImageRepository;

        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Guid> Handle(
        AddAstronomicalImageCommand request,
        CancellationToken cancellationToken)
    {
        var astronomicalObject =
            await _astronomicalObjectRepository.GetByIdAsync(
                request.AstronomicalObjectId,
                cancellationToken);

        if (astronomicalObject is null)
        {
            throw new KeyNotFoundException(
                "Astronomical object was not found.");
        }

        var image = AstronomicalImage.Create(
            request.AstronomicalObjectId,
            request.Url,
            request.Urls);

        await _astronomicalImageRepository.AddAsync(
            image,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _cache.RemoveAsync(
            $"star:{request.AstronomicalObjectId:N}:images",
            cancellationToken);

        return image.Id;
    }
}