using MediatR;
using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.ValueObjects;
using CosmicWiki.Caching;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Catalog.Application.Features.Stars.GetById;

public sealed class GetStarByIdQueryHandler
    : IRequestHandler<GetStarByIdQuery, StarDto?>
{
    private readonly IStarRepository _starRepository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetStarByIdQueryHandler(
        IStarRepository starRepository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _starRepository = starRepository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<StarDto?> Handle(
        GetStarByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"star:{request.Id:N}";
        var cached = await _cache.GetAsync<StarDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var star = await _starRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (star is null)
            return null;

        var result = new StarDto(
            star.Id,
            star.Name,

            star.ObjectTypes
                .Select(x => new AstronomicalObjectTypeDto(
                    x.Code,
                    x.Name,
                    x.Description))
                .ToList(),

            star.CatalogIdentifiers
                .Select(x => new CatalogIdentifierDto(
                    x.Catalog,
                    x.Value))
                .ToList(),

            star.Coordinates
                .Select(x => new AstronomicalCoordinateDto(
                    x.RightAscension,
                    x.Declination,
                    (int)x.CoordinateSystem,
                    x.Epoch,
                    x.WavelengthBand is null
                        ? null
                        : (int)x.WavelengthBand.Value,
                    x.Uncertainty is null
                        ? null
                        : new CoordinateUncertaintyDto(
                            x.Uncertainty.MajorAxis,
                            x.Uncertainty.MinorAxis,
                            x.Uncertainty.PositionAngle),
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.ProperMotions
                .Select(x => new ProperMotionDto(
                    x.RightAscension,
                    x.Declination,
                    x.Uncertainty is null
                        ? null
                        : new ErrorEllipseDto(
                            x.Uncertainty.MajorAxis,
                            x.Uncertainty.MinorAxis,
                            x.Uncertainty.PositionAngle),
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.Parallaxes
                .Select(x => new ParallaxDto(
                    x.ValueMas,
                    MapUncertainty(x.Uncertainty),
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.RadialVelocities
                .Select(x => new RadialVelocityMeasurementDto(
                    x.ValueKmPerSecond,
                    MapUncertainty(x.Uncertainty),
                    x.WavelengthBand is null
                        ? null
                        : (int)x.WavelengthBand.Value,
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.Distances
                .Select(x => new DistanceMeasurementDto(
                    x.Value,
                    x.Unit,
                    MapUncertainty(x.Uncertainty),
                    (int)x.Method,
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.SpectralClassifications
                .Select(x => new SpectralClassificationDto(
                    x.RawType,
                    x.TemperatureClass is null
                        ? null
                        : (int)x.TemperatureClass.Value,
                    x.Subtype,
                    x.LuminosityClass,
                    x.Peculiarities.ToList(),
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.PhotometricMeasurements
                .Select(x => new PhotometricMeasurementDto(
                    (int)x.Filter,
                    (int)x.System,
                    (int)x.ValueType,
                    MapMeasurement(x.Measurement),
                    x.Flags is null
                        ? null
                        : new PhotometryFlagsDto(
                            x.Flags.IsJoined,
                            x.Flags.VariabilityCode)))
                .ToList(),

            star.AtmosphericParameters
                .Select(x => new StellarAtmosphericParametersDto(
                    x.EffectiveTemperature is null
                        ? null
                        : new EffectiveTemperatureDto(
                            x.EffectiveTemperature.Kelvin),

                    x.SurfaceGravity is null
                        ? null
                        : new LogSurfaceGravityDto(
                            x.SurfaceGravity.Value),

                    x.Metallicity is null
                        ? null
                        : new MetallicityDto(
                            x.Metallicity.FeH),

                    (int)x.MetallicityFlags,
                    x.ComparisonStar,
                    x.CatalogNumber,
                    x.ReferenceId))
                .ToList(),

            star.Diameters
                .Select(x => new StellarDiameterDto(
                    x.Value,
                    x.Unit,
                    MapUncertainty(x.Uncertainty),
                    x.FilterOrWavelength,
                    x.Method,
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.ProjectedRotationalVelocities
                .Select(x => new ProjectedRotationalVelocityDto(
                    x.Value,
                    MapUncertainty(x.Uncertainty),
                    x.MeasurementCount,
                    (int)x.Quality,
                    x.ReferenceId))
                .ToList(),

            star.VariableStarMeasurements
                .Select(x => new VariableStarMeasurementDto(
                    x.VariabilityType,
                    x.MaximumBrightness,
                    x.MinimumBrightness,
                    x.PhotometricBand,
                    x.PeriodDays,
                    x.EpochJulianDay,
                    x.CharacteristicPercent,
                    x.ReferenceId))
                .ToList());

        var ttl = TimeSpan.FromSeconds(_cachingOptions.CatalogDetailTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);
        
        return result;
    }

    private static MeasurementUncertaintyDto? MapUncertainty(
        MeasurementUncertainty? uncertainty)
    {
        if (uncertainty is null)
            return null;

        return new MeasurementUncertaintyDto(
            uncertainty.Negative,
            uncertainty.Positive);
    }

    private static MeasurementDto MapMeasurement(
        Measurement measurement)
    {
        return new MeasurementDto(
            measurement.Value,
            measurement.Unit,
            MapUncertainty(measurement.Uncertainty),
            (int)measurement.Quality,
            measurement.ReferenceId);
    }
}