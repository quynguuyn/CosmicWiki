using CosmicWiki.Catalog.Domain.ValueObjects;
using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Catalog.Domain.Entities;

public sealed class Star : AggregateRoot<Guid>
{
    private readonly List<AstronomicalObjectType> _objectTypes = [];
    private readonly List<CatalogIdentifier> _catalogIdentifiers = [];
    private readonly List<AstronomicalCoordinate> _coordinates = [];
    private readonly List<ProperMotion> _properMotions = [];
    private readonly List<ParallaxMeasurement> _parallaxes = [];
    private readonly List<RadialVelocityMeasurement> _radialVelocities = [];
    private readonly List<DistanceMeasurement> _distances = [];
    private readonly List<SpectralClassification> _spectralClassifications = [];
    private readonly List<PhotometricMeasurement> _photometricMeasurements = [];
    private readonly List<StellarAtmosphericParameters> _atmosphericParameters = [];
    private readonly List<StellarDiameter> _diameters = [];
    private readonly List<ProjectedRotationalVelocity> _projectedRotationalVelocities = [];
    private readonly List<VariableStarMeasurement> _variableStarMeasurements = [];

    private Star(
        Guid id,
        string? name)
        : base(id)
    {
        Name = name;
    }

    public string? Name { get; private set; }

    public IReadOnlyCollection<AstronomicalObjectType> ObjectTypes =>
        _objectTypes.AsReadOnly();

    public IReadOnlyCollection<CatalogIdentifier> CatalogIdentifiers =>
        _catalogIdentifiers.AsReadOnly();

    public IReadOnlyCollection<AstronomicalCoordinate> Coordinates =>
        _coordinates.AsReadOnly();

    public IReadOnlyCollection<ProperMotion> ProperMotions =>
        _properMotions.AsReadOnly();

    public IReadOnlyCollection<ParallaxMeasurement> Parallaxes =>
        _parallaxes.AsReadOnly();

    public IReadOnlyCollection<RadialVelocityMeasurement> RadialVelocities =>
        _radialVelocities.AsReadOnly();

    public IReadOnlyCollection<DistanceMeasurement> Distances =>
        _distances.AsReadOnly();

    public IReadOnlyCollection<SpectralClassification> SpectralClassifications =>
        _spectralClassifications.AsReadOnly();

    public IReadOnlyCollection<PhotometricMeasurement> PhotometricMeasurements =>
        _photometricMeasurements.AsReadOnly();

    public IReadOnlyCollection<StellarAtmosphericParameters> AtmosphericParameters =>
        _atmosphericParameters.AsReadOnly();

    public IReadOnlyCollection<StellarDiameter> Diameters =>
        _diameters.AsReadOnly();

    public IReadOnlyCollection<ProjectedRotationalVelocity> ProjectedRotationalVelocities =>
        _projectedRotationalVelocities.AsReadOnly();

    public IReadOnlyCollection<VariableStarMeasurement> VariableStarMeasurements =>
        _variableStarMeasurements.AsReadOnly();

    public static Star Create(string? name = null)
    {
        return new Star(
            Guid.NewGuid(),
            name?.Trim());
    }
    
    public static Star Create(
        Guid id,
        string? name = null)
    {
        return new Star(
            id,
            name?.Trim());
    }

    public void Rename(string? name)
    {
        Name = name?.Trim();
    }

    public void AddObjectType(AstronomicalObjectType objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);

        if (_objectTypes.Contains(objectType))
            return;

        _objectTypes.Add(objectType);
    }

    public void AddCatalogIdentifier(CatalogIdentifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        if (_catalogIdentifiers.Any(x => x.Matches(identifier)))
            return;

        _catalogIdentifiers.Add(identifier);
    }

    public bool HasCatalogIdentifier(
        string catalog,
        string value)
    {
        if (string.IsNullOrWhiteSpace(catalog))
            throw new ArgumentException(
                "Catalog cannot be empty.",
                nameof(catalog));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Identifier value cannot be empty.",
                nameof(value));

        return _catalogIdentifiers.Any(x =>
            string.Equals(
                x.Catalog,
                catalog.Trim(),
                StringComparison.OrdinalIgnoreCase)
            &&
            string.Equals(
                x.Value,
                value.Trim(),
                StringComparison.OrdinalIgnoreCase));
    }

    public void AddCoordinate(AstronomicalCoordinate coordinate)
    {
        ArgumentNullException.ThrowIfNull(coordinate);
        _coordinates.Add(coordinate);
    }

    public void AddProperMotion(ProperMotion properMotion)
    {
        ArgumentNullException.ThrowIfNull(properMotion);
        _properMotions.Add(properMotion);
    }

    public void AddParallax(ParallaxMeasurement parallax)
    {
        ArgumentNullException.ThrowIfNull(parallax);
        _parallaxes.Add(parallax);
    }

    public void AddRadialVelocity(
        RadialVelocityMeasurement radialVelocity)
    {
        ArgumentNullException.ThrowIfNull(radialVelocity);
        _radialVelocities.Add(radialVelocity);
    }

    public void AddDistance(DistanceMeasurement distance)
    {
        ArgumentNullException.ThrowIfNull(distance);
        _distances.Add(distance);
    }

    public void AddSpectralClassification(
        SpectralClassification classification)
    {
        ArgumentNullException.ThrowIfNull(classification);
        _spectralClassifications.Add(classification);
    }

    public void AddPhotometry(
        PhotometricMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);
        _photometricMeasurements.Add(measurement);
    }

    public void AddAtmosphericParameters(
        StellarAtmosphericParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        _atmosphericParameters.Add(parameters);
    }

    public void AddDiameter(StellarDiameter diameter)
    {
        ArgumentNullException.ThrowIfNull(diameter);
        _diameters.Add(diameter);
    }

    public void AddProjectedRotationalVelocity(
        ProjectedRotationalVelocity velocity)
    {
        ArgumentNullException.ThrowIfNull(velocity);
        _projectedRotationalVelocities.Add(velocity);
    }

    public void AddVariableStarMeasurement(
        VariableStarMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);
        _variableStarMeasurements.Add(measurement);
    }
    
    public void ReplaceCatalogIdentifiers(
        IEnumerable<CatalogIdentifier> identifiers)
    {
        ArgumentNullException.ThrowIfNull(identifiers);

        _catalogIdentifiers.Clear();

        foreach (var identifier in identifiers)
        {
            AddCatalogIdentifier(identifier);
        }
    }
    
    public void ReplaceParallaxes(
        IEnumerable<ParallaxMeasurement> parallaxes)
    {
        ArgumentNullException.ThrowIfNull(parallaxes);

        _parallaxes.Clear();

        foreach (var parallax in parallaxes)
        {
            AddParallax(parallax);
        }
    }
}