using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteStarScientificMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_CATALOG_IDENTIFIERS",
                newName: "VALUE");

            migrationBuilder.RenameColumn(
                name: "Catalog",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_CATALOG_IDENTIFIERS",
                newName: "CATALOG");

            migrationBuilder.AddColumn<double>(
                name: "UNCERTAINTY_NEGATIVE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PARALLAXES",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UNCERTAINTY_POSITIVE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PARALLAXES",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UNCERTAINTY_MAJOR_AXIS",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UNCERTAINTY_MINOR_AXIS",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UNCERTAINTY_POSITION_ANGLE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "STAR_ATMOSPHERIC_PARAMETERS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EFFECTIVE_TEMPERATURE_KELVIN = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    LOG_SURFACE_GRAVITY = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    METALLICITY_FE_H = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    METALLICITY_FLAGS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    COMPARISON_STAR = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CATALOG_NUMBER = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_ATMOSPHERIC_PARAMETERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_ATMOSPHERIC_PARAMETERS_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_DIAMETERS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VALUE = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UNIT = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    UNCERTAINTY_NEGATIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_POSITIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    FILTER_OR_WAVELENGTH = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    METHOD = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_DIAMETERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_DIAMETERS_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_DISTANCES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VALUE = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UNIT = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    UNCERTAINTY_NEGATIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_POSITIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    METHOD = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_DISTANCES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_DISTANCES_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_OBJECT_TYPES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_OBJECT_TYPES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_OBJECT_TYPES_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_PHOTOMETRY",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FILTER = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SYSTEM = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VALUE_TYPE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VALUE = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UNIT = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    UNCERTAINTY_NEGATIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_POSITIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    IS_JOINED = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    VARIABILITY_CODE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_PHOTOMETRY", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_PHOTOMETRY_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_RADIAL_VELOCITIES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VALUE_KM_PER_SECOND = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UNCERTAINTY_NEGATIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_POSITIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    WAVELENGTH_BAND = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_RADIAL_VELOCITIES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_RADIAL_VELOCITIES_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_ROTATIONAL_VELOCITIES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VALUE_KM_PER_SECOND = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UNCERTAINTY_NEGATIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_POSITIVE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    MEASUREMENT_COUNT = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_ROTATIONAL_VELOCITIES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_ROTATIONAL_VELOCITIES_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_SPECTRAL_CLASSIFICATIONS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RAW_TYPE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    TEMPERATURE_CLASS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SUBTYPE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    LUMINOSITY_CLASS = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_SPECTRAL_CLASSIFICATIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_SPECTRAL_CLASSIFICATIONS_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_VARIABLE_MEASUREMENTS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VARIABILITY_TYPE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    MAXIMUM_BRIGHTNESS = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    MINIMUM_BRIGHTNESS = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    PHOTOMETRIC_BAND = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    PERIOD_DAYS = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    EPOCH_JULIAN_DAY = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    CHARACTERISTIC_PERCENT = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_VARIABLE_MEASUREMENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_VARIABLE_MEASUREMENTS_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_STAR_ATMOSPHERIC_PARAMETERS_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_ATMOSPHERIC_PARAMETERS",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_ATMOSPHERIC_PARAMETERS_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_ATMOSPHERIC_PARAMETERS",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_DIAMETERS_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_DIAMETERS",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_DIAMETERS_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_DIAMETERS",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_DISTANCES_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_DISTANCES",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_DISTANCES_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_DISTANCES",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_OBJECT_TYPES_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_OBJECT_TYPES",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_OBJECT_TYPES_STAR_ID_CODE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_OBJECT_TYPES",
                columns: new[] { "STAR_ID", "CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_STAR_PHOTOMETRY_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PHOTOMETRY",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_RADIAL_VELOCITIES_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_RADIAL_VELOCITIES",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_RADIAL_VELOCITIES_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_RADIAL_VELOCITIES",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_ROTATIONAL_VELOCITIES_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_ROTATIONAL_VELOCITIES",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_ROTATIONAL_VELOCITIES_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_ROTATIONAL_VELOCITIES",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_SPECTRAL_CLASSIFICATIONS_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_SPECTRAL_CLASSIFICATIONS",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_SPECTRAL_CLASSIFICATIONS_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_SPECTRAL_CLASSIFICATIONS",
                column: "STAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_VARIABLE_MEASUREMENTS_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_VARIABLE_MEASUREMENTS",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_VARIABLE_MEASUREMENTS_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_VARIABLE_MEASUREMENTS",
                column: "STAR_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STAR_ATMOSPHERIC_PARAMETERS",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_DIAMETERS",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_DISTANCES",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_OBJECT_TYPES",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_PHOTOMETRY",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_RADIAL_VELOCITIES",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_ROTATIONAL_VELOCITIES",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_SPECTRAL_CLASSIFICATIONS",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_VARIABLE_MEASUREMENTS",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropColumn(
                name: "UNCERTAINTY_NEGATIVE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PARALLAXES");

            migrationBuilder.DropColumn(
                name: "UNCERTAINTY_POSITIVE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PARALLAXES");

            migrationBuilder.DropColumn(
                name: "UNCERTAINTY_MAJOR_AXIS",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES");

            migrationBuilder.DropColumn(
                name: "UNCERTAINTY_MINOR_AXIS",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES");

            migrationBuilder.DropColumn(
                name: "UNCERTAINTY_POSITION_ANGLE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES");

            migrationBuilder.RenameColumn(
                name: "VALUE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_CATALOG_IDENTIFIERS",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "CATALOG",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_CATALOG_IDENTIFIERS",
                newName: "Catalog");
        }
    }
}
