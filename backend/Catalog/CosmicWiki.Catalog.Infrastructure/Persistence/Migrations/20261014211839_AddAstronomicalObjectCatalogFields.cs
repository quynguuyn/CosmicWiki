using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAstronomicalObjectCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "CATALOG", "NVARCHAR2(50)");
            AddColumnIfMissing(migrationBuilder, "CATALOG_VALUE", "NVARCHAR2(200)");
            AddColumnIfMissing(migrationBuilder, "DEC_DEGREES", "BINARY_DOUBLE");
            AddColumnIfMissing(
                migrationBuilder,
                "KIND",
                "NVARCHAR2(30) DEFAULT ''Other'' NOT NULL");
            AddColumnIfMissing(migrationBuilder, "NAME", "NVARCHAR2(500)");
            AddColumnIfMissing(migrationBuilder, "OBJECT_TYPE", "NVARCHAR2(200)");
            AddColumnIfMissing(migrationBuilder, "RA_DEGREES", "BINARY_DOUBLE");

            migrationBuilder.Sql(
                """
                UPDATE COSMICWIKI_CATALOG.ASTRONOMICAL_OBJECTS AO
                SET NAME = (
                        SELECT S."Name"
                        FROM COSMICWIKI_CATALOG.STARS S
                        WHERE S."Id" = AO.ID),
                    KIND = 'Star',
                    OBJECT_TYPE = (
                        SELECT T.NAME
                        FROM COSMICWIKI_CATALOG.STAR_OBJECT_TYPES T
                        WHERE T.STAR_ID = AO.ID
                          AND ROWNUM = 1),
                    CATALOG = (
                        SELECT MIN(I.CATALOG)
                            KEEP (DENSE_RANK FIRST ORDER BY I.CATALOG, I.VALUE)
                        FROM COSMICWIKI_CATALOG.STAR_CATALOG_IDENTIFIERS I
                        WHERE I.STAR_ID = AO.ID),
                    CATALOG_VALUE = (
                        SELECT MIN(I.VALUE)
                            KEEP (DENSE_RANK FIRST ORDER BY I.CATALOG, I.VALUE)
                        FROM COSMICWIKI_CATALOG.STAR_CATALOG_IDENTIFIERS I
                        WHERE I.STAR_ID = AO.ID),
                    RA_DEGREES = (
                        SELECT C.RIGHT_ASCENSION
                        FROM COSMICWIKI_CATALOG.STAR_COORDINATES C
                        WHERE C.STAR_ID = AO.ID
                          AND ROWNUM = 1),
                    DEC_DEGREES = (
                        SELECT C.DECLINATION
                        FROM COSMICWIKI_CATALOG.STAR_COORDINATES C
                        WHERE C.STAR_ID = AO.ID
                          AND ROWNUM = 1)
                WHERE EXISTS (
                    SELECT 1
                    FROM COSMICWIKI_CATALOG.STARS S
                    WHERE S."Id" = AO.ID)
                """);

            migrationBuilder.Sql(
                """
                DECLARE
                    index_count NUMBER;
                BEGIN
                    SELECT COUNT(*)
                    INTO index_count
                    FROM ALL_INDEXES
                    WHERE OWNER = 'COSMICWIKI_CATALOG'
                      AND INDEX_NAME = 'IX_ASTRONOMICAL_OBJECTS_CATALOG_CATALOG_VALUE';

                    IF index_count = 0 THEN
                        EXECUTE IMMEDIATE
                            'CREATE UNIQUE INDEX COSMICWIKI_CATALOG.IX_ASTRONOMICAL_OBJECTS_CATALOG_CATALOG_VALUE ' ||
                            'ON COSMICWIKI_CATALOG.ASTRONOMICAL_OBJECTS ("CATALOG", "CATALOG_VALUE")';
                    END IF;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ASTRONOMICAL_OBJECTS_CATALOG_CATALOG_VALUE",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "CATALOG",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "CATALOG_VALUE",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "DEC_DEGREES",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "KIND",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "NAME",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "OBJECT_TYPE",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");

            migrationBuilder.DropColumn(
                name: "RA_DEGREES",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_OBJECTS");
        }

        private static void AddColumnIfMissing(
            MigrationBuilder migrationBuilder,
            string columnName,
            string columnDefinition)
        {
            migrationBuilder.Sql(
                $"""
                DECLARE
                    column_count NUMBER;
                BEGIN
                    SELECT COUNT(*)
                    INTO column_count
                    FROM ALL_TAB_COLUMNS
                    WHERE OWNER = 'COSMICWIKI_CATALOG'
                      AND TABLE_NAME = 'ASTRONOMICAL_OBJECTS'
                      AND COLUMN_NAME = '{columnName}';

                    IF column_count = 0 THEN
                        EXECUTE IMMEDIATE
                            'ALTER TABLE COSMICWIKI_CATALOG.ASTRONOMICAL_OBJECTS ' ||
                            'ADD ("{columnName}" {columnDefinition})';
                    END IF;
                END;
                """);
        }
    }
}
