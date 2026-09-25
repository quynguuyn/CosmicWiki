using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Configurations;

public sealed class StarConfiguration : IEntityTypeConfiguration<Star>
{
    public void Configure(EntityTypeBuilder<Star> builder)
    {
        builder.ToTable("STARS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(500);
        
        builder.HasOne<AstronomicalObject>()
            .WithOne()
            .HasForeignKey<Star>(x => x.Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        ConfigureObjectTypes(builder);
        ConfigureCatalogIdentifiers(builder);
        ConfigureCoordinates(builder);
        ConfigureProperMotions(builder);
        ConfigureParallaxes(builder);
        ConfigureRadialVelocities(builder);
        ConfigureDistances(builder);
        ConfigureSpectralClassifications(builder);
        ConfigurePhotometry(builder);
        ConfigureAtmosphericParameters(builder);
        ConfigureDiameters(builder);
        ConfigureProjectedRotationalVelocities(builder);
        ConfigureVariableStarMeasurements(builder);
    }

    private static void ConfigureObjectTypes(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.ObjectTypes,
            owned =>
            {
                owned.ToTable("STAR_OBJECT_TYPES");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<string>(
                        nameof(AstronomicalObjectType.Code))
                    .HasColumnName("CODE")
                    .HasMaxLength(50)
                    .IsRequired();

                owned.Property<string>(
                        nameof(AstronomicalObjectType.Name))
                    .HasColumnName("NAME")
                    .HasMaxLength(200)
                    .IsRequired();

                owned.Property<string?>(
                        nameof(AstronomicalObjectType.Description))
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(1000);

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.WithOwner()
                    .HasForeignKey("StarId");

                owned.HasIndex("StarId");

                owned.HasIndex(
                        "StarId",
                        nameof(AstronomicalObjectType.Code))
                    .IsUnique();
            });

        builder.Navigation(x => x.ObjectTypes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureCatalogIdentifiers(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.CatalogIdentifiers,
            owned =>
            {
                owned.ToTable("STAR_CATALOG_IDENTIFIERS");

                owned.HasKey(
                    "StarId",
                    nameof(CatalogIdentifier.Catalog),
                    nameof(CatalogIdentifier.Value));

                owned.HasIndex(
                        nameof(CatalogIdentifier.Catalog),
                        nameof(CatalogIdentifier.Value))
                    .IsUnique();

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.Property<string>(
                        nameof(CatalogIdentifier.Catalog))
                    .HasColumnName("CATALOG")
                    .HasMaxLength(50)
                    .IsRequired();

                owned.Property<string>(
                        nameof(CatalogIdentifier.Value))
                    .HasColumnName("VALUE")
                    .HasMaxLength(200)
                    .IsRequired();

                owned.WithOwner()
                    .HasForeignKey("StarId");
            });

        builder.Navigation(x => x.CatalogIdentifiers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureCoordinates(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.Coordinates,
            owned =>
            {
                owned.ToTable("STAR_COORDINATES");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(AstronomicalCoordinate.RightAscension))
                    .HasColumnName("RIGHT_ASCENSION")
                    .IsRequired();

                owned.Property<double>(
                        nameof(AstronomicalCoordinate.Declination))
                    .HasColumnName("DECLINATION")
                    .IsRequired();

                owned.Property<CoordinateSystem>(
                        nameof(AstronomicalCoordinate.CoordinateSystem))
                    .HasColumnName("COORDINATE_SYSTEM")
                    .IsRequired();

                owned.Property<double>(
                        nameof(AstronomicalCoordinate.Epoch))
                    .HasColumnName("EPOCH")
                    .IsRequired();

                owned.Property<WavelengthBand?>(
                        nameof(AstronomicalCoordinate.WavelengthBand))
                    .HasColumnName("WAVELENGTH_BAND");

                owned.Property<MeasurementQuality>(
                        nameof(AstronomicalCoordinate.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(AstronomicalCoordinate.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(CoordinateUncertainty),
                    nameof(AstronomicalCoordinate.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double>(
                                nameof(CoordinateUncertainty.MajorAxis))
                            .HasColumnName("UNCERTAINTY_MAJOR_AXIS");

                        uncertainty.Property<double>(
                                nameof(CoordinateUncertainty.MinorAxis))
                            .HasColumnName("UNCERTAINTY_MINOR_AXIS");

                        uncertainty.Property<double>(
                                nameof(CoordinateUncertainty.PositionAngle))
                            .HasColumnName("UNCERTAINTY_POSITION_ANGLE");
                    });
            });

        builder.Navigation(x => x.Coordinates)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureProperMotions(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.ProperMotions,
            owned =>
            {
                owned.ToTable("STAR_PROPER_MOTIONS");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(ProperMotion.RightAscension))
                    .HasColumnName("RIGHT_ASCENSION")
                    .IsRequired();

                owned.Property<double>(
                        nameof(ProperMotion.Declination))
                    .HasColumnName("DECLINATION")
                    .IsRequired();

                owned.Property<MeasurementQuality>(
                        nameof(ProperMotion.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(ProperMotion.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(ErrorEllipse),
                    nameof(ProperMotion.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double>(
                                nameof(ErrorEllipse.MajorAxis))
                            .HasColumnName("UNCERTAINTY_MAJOR_AXIS");

                        uncertainty.Property<double>(
                                nameof(ErrorEllipse.MinorAxis))
                            .HasColumnName("UNCERTAINTY_MINOR_AXIS");

                        uncertainty.Property<double>(
                                nameof(ErrorEllipse.PositionAngle))
                            .HasColumnName("UNCERTAINTY_POSITION_ANGLE");
                    });
            });

        builder.Navigation(x => x.ProperMotions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureParallaxes(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.Parallaxes,
            owned =>
            {
                owned.ToTable("STAR_PARALLAXES");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(ParallaxMeasurement.ValueMas))
                    .HasColumnName("VALUE_MAS")
                    .IsRequired();

                owned.Property<MeasurementQuality>(
                        nameof(ParallaxMeasurement.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(ParallaxMeasurement.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(MeasurementUncertainty),
                    nameof(ParallaxMeasurement.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Negative))
                            .HasColumnName("UNCERTAINTY_NEGATIVE");

                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Positive))
                            .HasColumnName("UNCERTAINTY_POSITIVE");
                    });
            });

        builder.Navigation(x => x.Parallaxes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureRadialVelocities(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.RadialVelocities,
            owned =>
            {
                owned.ToTable("STAR_RADIAL_VELOCITIES");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(RadialVelocityMeasurement.ValueKmPerSecond))
                    .HasColumnName("VALUE_KM_PER_SECOND")
                    .IsRequired();

                owned.Property<WavelengthBand?>(
                        nameof(RadialVelocityMeasurement.WavelengthBand))
                    .HasColumnName("WAVELENGTH_BAND");

                owned.Property<MeasurementQuality>(
                        nameof(RadialVelocityMeasurement.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(RadialVelocityMeasurement.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(MeasurementUncertainty),
                    nameof(RadialVelocityMeasurement.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Negative))
                            .HasColumnName("UNCERTAINTY_NEGATIVE");

                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Positive))
                            .HasColumnName("UNCERTAINTY_POSITIVE");
                    });
            });

        builder.Navigation(x => x.RadialVelocities)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureDistances(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.Distances,
            owned =>
            {
                owned.ToTable("STAR_DISTANCES");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(DistanceMeasurement.Value))
                    .HasColumnName("VALUE")
                    .IsRequired();

                owned.Property<string>(
                        nameof(DistanceMeasurement.Unit))
                    .HasColumnName("UNIT")
                    .HasMaxLength(50)
                    .IsRequired();

                owned.Property<DistanceMethod>(
                        nameof(DistanceMeasurement.Method))
                    .HasColumnName("METHOD")
                    .IsRequired();

                owned.Property<MeasurementQuality>(
                        nameof(DistanceMeasurement.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(DistanceMeasurement.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(MeasurementUncertainty),
                    nameof(DistanceMeasurement.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Negative))
                            .HasColumnName("UNCERTAINTY_NEGATIVE");

                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Positive))
                            .HasColumnName("UNCERTAINTY_POSITIVE");
                    });
            });

        builder.Navigation(x => x.Distances)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureSpectralClassifications(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.SpectralClassifications,
            owned =>
            {
                owned.ToTable("STAR_SPECTRAL_CLASSIFICATIONS");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<string>(
                        nameof(SpectralClassification.RawType))
                    .HasColumnName("RAW_TYPE")
                    .HasMaxLength(200)
                    .IsRequired();

                owned.Property<SpectralTemperatureClass?>(
                        nameof(SpectralClassification.TemperatureClass))
                    .HasColumnName("TEMPERATURE_CLASS");

                owned.Property<string?>(
                        nameof(SpectralClassification.Subtype))
                    .HasColumnName("SUBTYPE")
                    .HasMaxLength(50);

                owned.Property<string?>(
                        nameof(SpectralClassification.LuminosityClass))
                    .HasColumnName("LUMINOSITY_CLASS")
                    .HasMaxLength(50);

                owned.Property<MeasurementQuality>(
                        nameof(SpectralClassification.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(SpectralClassification.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                // Peculiarities is a collection of strings.
                // It is intentionally not persisted in this slice.
                owned.Ignore(
                    nameof(SpectralClassification.Peculiarities));
            });

        builder.Navigation(x => x.SpectralClassifications)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigurePhotometry(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.PhotometricMeasurements,
            owned =>
            {
                owned.ToTable("STAR_PHOTOMETRY");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<PhotometricFilter>(
                        nameof(PhotometricMeasurement.Filter))
                    .HasColumnName("FILTER")
                    .IsRequired();

                owned.Property<PhotometricSystem>(
                        nameof(PhotometricMeasurement.System))
                    .HasColumnName("SYSTEM")
                    .IsRequired();

                owned.Property<PhotometricValueType>(
                        nameof(PhotometricMeasurement.ValueType))
                    .HasColumnName("VALUE_TYPE")
                    .IsRequired();

                owned.OwnsOne(
                    typeof(PhotometryFlags),
                    nameof(PhotometricMeasurement.Flags),
                    flags =>
                    {
                        flags.Property<bool>(
                                nameof(PhotometryFlags.IsJoined))
                            .HasColumnName("IS_JOINED")
                            .IsRequired();

                        flags.Property<string?>(
                                nameof(PhotometryFlags.VariabilityCode))
                            .HasColumnName("VARIABILITY_CODE")
                            .HasMaxLength(50);
                    });

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");

                owned.OwnsOne(
                    typeof(Measurement),
                    nameof(PhotometricMeasurement.Measurement),
                    measurement =>
                    {
                        measurement.Property<double>(
                                nameof(Measurement.Value))
                            .HasColumnName("VALUE")
                            .IsRequired();

                        measurement.Property<string>(
                                nameof(Measurement.Unit))
                            .HasColumnName("UNIT")
                            .HasMaxLength(50)
                            .IsRequired();

                        measurement.Property<MeasurementQuality>(
                                nameof(Measurement.Quality))
                            .HasColumnName("QUALITY")
                            .IsRequired();

                        measurement.Property<Guid?>(
                                nameof(Measurement.ReferenceId))
                            .HasColumnName("REFERENCE_ID");

                        measurement.OwnsOne(
                            typeof(MeasurementUncertainty),
                            nameof(Measurement.Uncertainty),
                            uncertainty =>
                            {
                                uncertainty.Property<double?>(
                                        nameof(MeasurementUncertainty.Negative))
                                    .HasColumnName(
                                        "UNCERTAINTY_NEGATIVE");

                                uncertainty.Property<double?>(
                                        nameof(MeasurementUncertainty.Positive))
                                    .HasColumnName(
                                        "UNCERTAINTY_POSITIVE");
                            });
                    });
            });

        builder.Navigation(x => x.PhotometricMeasurements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureAtmosphericParameters(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.AtmosphericParameters,
            owned =>
            {
                owned.ToTable("STAR_ATMOSPHERIC_PARAMETERS");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<MetallicityFlags>(
                        nameof(StellarAtmosphericParameters.MetallicityFlags))
                    .HasColumnName("METALLICITY_FLAGS")
                    .IsRequired();

                owned.Property<string?>(
                        nameof(StellarAtmosphericParameters.ComparisonStar))
                    .HasColumnName("COMPARISON_STAR")
                    .HasMaxLength(200);

                owned.Property<string?>(
                        nameof(StellarAtmosphericParameters.CatalogNumber))
                    .HasColumnName("CATALOG_NUMBER")
                    .HasMaxLength(100);

                owned.Property<Guid?>(
                        nameof(StellarAtmosphericParameters.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(EffectiveTemperature),
                    nameof(StellarAtmosphericParameters.EffectiveTemperature),
                    temperature =>
                    {
                        temperature.Property<double>(
                                nameof(EffectiveTemperature.Kelvin))
                            .HasColumnName(
                                "EFFECTIVE_TEMPERATURE_KELVIN");
                    });

                owned.OwnsOne(
                    typeof(LogSurfaceGravity),
                    nameof(StellarAtmosphericParameters.SurfaceGravity),
                    gravity =>
                    {
                        gravity.Property<double>(
                                nameof(LogSurfaceGravity.Value))
                            .HasColumnName("LOG_SURFACE_GRAVITY");
                    });

                owned.OwnsOne(
                    typeof(Metallicity),
                    nameof(StellarAtmosphericParameters.Metallicity),
                    metallicity =>
                    {
                        metallicity.Property<double>(
                                nameof(Metallicity.FeH))
                            .HasColumnName("METALLICITY_FE_H");
                    });
            });

        builder.Navigation(x => x.AtmosphericParameters)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureDiameters(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.Diameters,
            owned =>
            {
                owned.ToTable("STAR_DIAMETERS");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(StellarDiameter.Value))
                    .HasColumnName("VALUE")
                    .IsRequired();

                owned.Property<string>(
                        nameof(StellarDiameter.Unit))
                    .HasColumnName("UNIT")
                    .HasMaxLength(50)
                    .IsRequired();

                owned.Property<string?>(
                        nameof(StellarDiameter.FilterOrWavelength))
                    .HasColumnName("FILTER_OR_WAVELENGTH")
                    .HasMaxLength(100);

                owned.Property<string?>(
                        nameof(StellarDiameter.Method))
                    .HasColumnName("METHOD")
                    .HasMaxLength(200);

                owned.Property<MeasurementQuality>(
                        nameof(StellarDiameter.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(StellarDiameter.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(MeasurementUncertainty),
                    nameof(StellarDiameter.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Negative))
                            .HasColumnName("UNCERTAINTY_NEGATIVE");

                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Positive))
                            .HasColumnName("UNCERTAINTY_POSITIVE");
                    });
            });

        builder.Navigation(x => x.Diameters)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureProjectedRotationalVelocities(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.ProjectedRotationalVelocities,
            owned =>
            {
                owned.ToTable("STAR_ROTATIONAL_VELOCITIES");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<double>(
                        nameof(ProjectedRotationalVelocity.Value))
                    .HasColumnName("VALUE_KM_PER_SECOND")
                    .IsRequired();

                owned.Property<int?>(
                        nameof(ProjectedRotationalVelocity.MeasurementCount))
                    .HasColumnName("MEASUREMENT_COUNT");

                owned.Property<MeasurementQuality>(
                        nameof(ProjectedRotationalVelocity.Quality))
                    .HasColumnName("QUALITY")
                    .IsRequired();

                owned.Property<Guid?>(
                        nameof(ProjectedRotationalVelocity.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");

                owned.OwnsOne(
                    typeof(MeasurementUncertainty),
                    nameof(ProjectedRotationalVelocity.Uncertainty),
                    uncertainty =>
                    {
                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Negative))
                            .HasColumnName("UNCERTAINTY_NEGATIVE");

                        uncertainty.Property<double?>(
                                nameof(MeasurementUncertainty.Positive))
                            .HasColumnName("UNCERTAINTY_POSITIVE");
                    });
            });

        builder.Navigation(x => x.ProjectedRotationalVelocities)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureVariableStarMeasurements(
        EntityTypeBuilder<Star> builder)
    {
        builder.OwnsMany(
            x => x.VariableStarMeasurements,
            owned =>
            {
                owned.ToTable("STAR_VARIABLE_MEASUREMENTS");

                owned.Property<Guid>("Id")
                    .HasColumnName("ID")
                    .IsRequired();

                owned.HasKey("Id");

                owned.Property<string>(
                        nameof(VariableStarMeasurement.VariabilityType))
                    .HasColumnName("VARIABILITY_TYPE")
                    .HasMaxLength(100)
                    .IsRequired();

                owned.Property<double?>(
                        nameof(VariableStarMeasurement.MaximumBrightness))
                    .HasColumnName("MAXIMUM_BRIGHTNESS");

                owned.Property<double?>(
                        nameof(VariableStarMeasurement.MinimumBrightness))
                    .HasColumnName("MINIMUM_BRIGHTNESS");

                owned.Property<string?>(
                        nameof(VariableStarMeasurement.PhotometricBand))
                    .HasColumnName("PHOTOMETRIC_BAND")
                    .HasMaxLength(50);

                owned.Property<double?>(
                        nameof(VariableStarMeasurement.PeriodDays))
                    .HasColumnName("PERIOD_DAYS");

                owned.Property<double?>(
                        nameof(VariableStarMeasurement.EpochJulianDay))
                    .HasColumnName("EPOCH_JULIAN_DAY");

                owned.Property<double?>(
                        nameof(VariableStarMeasurement.CharacteristicPercent))
                    .HasColumnName("CHARACTERISTIC_PERCENT");

                owned.Property<Guid?>(
                        nameof(VariableStarMeasurement.ReferenceId))
                    .HasColumnName("REFERENCE_ID");

                owned.Property<Guid>("StarId")
                    .HasColumnName("STAR_ID")
                    .IsRequired();

                owned.HasIndex("StarId");
                owned.HasIndex("ReferenceId");
            });

        builder.Navigation(x => x.VariableStarMeasurements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}