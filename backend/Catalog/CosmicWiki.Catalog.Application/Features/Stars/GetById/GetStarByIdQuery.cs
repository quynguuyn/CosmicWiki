using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.GetById;

public sealed record GetStarByIdQuery(Guid Id) : IRequest<StarDto?>;

public sealed record StarDto(
    Guid Id,
    string? Name,
    IReadOnlyCollection<AstronomicalObjectTypeDto> ObjectTypes,
    IReadOnlyCollection<CatalogIdentifierDto> CatalogIdentifiers,
    IReadOnlyCollection<AstronomicalCoordinateDto> Coordinates,
    IReadOnlyCollection<ProperMotionDto> ProperMotions,
    IReadOnlyCollection<ParallaxDto> Parallaxes,
    IReadOnlyCollection<RadialVelocityMeasurementDto> RadialVelocities,
    IReadOnlyCollection<DistanceMeasurementDto> Distances,
    IReadOnlyCollection<SpectralClassificationDto> SpectralClassifications,
    IReadOnlyCollection<PhotometricMeasurementDto> PhotometricMeasurements,
    IReadOnlyCollection<StellarAtmosphericParametersDto> AtmosphericParameters,
    IReadOnlyCollection<StellarDiameterDto> Diameters,
    IReadOnlyCollection<ProjectedRotationalVelocityDto> ProjectedRotationalVelocities,
    IReadOnlyCollection<VariableStarMeasurementDto> VariableStarMeasurements);

public sealed record AstronomicalObjectTypeDto(
    string Code,
    string Name,
    string? Description);

public sealed record CatalogIdentifierDto(
    string Catalog,
    string Value);

public sealed record AstronomicalCoordinateDto(
    double RightAscension,
    double Declination,
    int CoordinateSystem,
    double Epoch,
    int? WavelengthBand,
    CoordinateUncertaintyDto? Uncertainty,
    int Quality,
    Guid? ReferenceId);

public sealed record CoordinateUncertaintyDto(
    double MajorAxis,
    double MinorAxis,
    double PositionAngle);

public sealed record ProperMotionDto(
    double RightAscension,
    double Declination,
    ErrorEllipseDto? Uncertainty,
    int Quality,
    Guid? ReferenceId);

public sealed record ErrorEllipseDto(
    double MajorAxis,
    double MinorAxis,
    double PositionAngle);

public sealed record ParallaxDto(
    double ValueMas,
    MeasurementUncertaintyDto? Uncertainty,
    int Quality,
    Guid? ReferenceId);

public sealed record RadialVelocityMeasurementDto(
    double ValueKmPerSecond,
    MeasurementUncertaintyDto? Uncertainty,
    int? WavelengthBand,
    int Quality,
    Guid? ReferenceId);

public sealed record DistanceMeasurementDto(
    double Value,
    string Unit,
    MeasurementUncertaintyDto? Uncertainty,
    int Method,
    int Quality,
    Guid? ReferenceId);

public sealed record SpectralClassificationDto(
    string RawType,
    int? TemperatureClass,
    string? Subtype,
    string? LuminosityClass,
    IReadOnlyCollection<string> Peculiarities,
    int Quality,
    Guid? ReferenceId);

public sealed record PhotometricMeasurementDto(
    int Filter,
    int System,
    int ValueType,
    MeasurementDto Measurement,
    PhotometryFlagsDto? Flags);

public sealed record MeasurementDto(
    double Value,
    string Unit,
    MeasurementUncertaintyDto? Uncertainty,
    int Quality,
    Guid? ReferenceId);

public sealed record PhotometryFlagsDto(
    bool IsJoined,
    string? VariabilityCode);

public sealed record MeasurementUncertaintyDto(
    double? Negative,
    double? Positive);

public sealed record StellarAtmosphericParametersDto(
    EffectiveTemperatureDto? EffectiveTemperature,
    LogSurfaceGravityDto? SurfaceGravity,
    MetallicityDto? Metallicity,
    int MetallicityFlags,
    string? ComparisonStar,
    string? CatalogNumber,
    Guid? ReferenceId);

public sealed record EffectiveTemperatureDto(
    double Kelvin);

public sealed record LogSurfaceGravityDto(
    double Value);

public sealed record MetallicityDto(
    double FeH);

public sealed record StellarDiameterDto(
    double Value,
    string Unit,
    MeasurementUncertaintyDto? Uncertainty,
    string? FilterOrWavelength,
    string? Method,
    int Quality,
    Guid? ReferenceId);

public sealed record ProjectedRotationalVelocityDto(
    double Value,
    MeasurementUncertaintyDto? Uncertainty,
    int? MeasurementCount,
    int Quality,
    Guid? ReferenceId);

public sealed record VariableStarMeasurementDto(
    string VariabilityType,
    double? MaximumBrightness,
    double? MinimumBrightness,
    string? PhotometricBand,
    double? PeriodDays,
    double? EpochJulianDay,
    double? CharacteristicPercent,
    Guid? ReferenceId);