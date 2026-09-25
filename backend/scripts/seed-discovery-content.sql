-- Run as COSMICWIKI_DISCOVERY after the Discovery migrations.
-- Idempotent archive records used by the Library and Home pages.

MERGE INTO SEARCH_DOCUMENTS target
USING (
    SELECT '20000000-0000-0000-0000-000000000001' SOURCE_ID,
           'galaxy' SOURCE_TYPE, 'Andromeda Galaxy (M31)' NAME,
           'The nearest large galaxy to the Milky Way, approximately 2.5 million light-years away.' DESCRIPTION,
           'https://upload.wikimedia.org/wikipedia/commons/a/ae/Andromeda_Galaxy_%28with_h-alpha%29.jpg' IMAGE_URL FROM DUAL
    UNION ALL SELECT '20000000-0000-0000-0000-000000000002', 'planet', 'Jupiter',
           'The largest planet in the Solar System, known for its Great Red Spot and many moons.',
           'https://upload.wikimedia.org/wikipedia/commons/e/e2/Jupiter.jpg' FROM DUAL
    UNION ALL SELECT '20000000-0000-0000-0000-000000000003', 'solar-system', 'Saturn',
           'A gas giant distinguished by its extensive ring system and low density.',
           'https://upload.wikimedia.org/wikipedia/commons/c/c7/Saturn_during_Equinox.jpg' FROM DUAL
    UNION ALL SELECT '20000000-0000-0000-0000-000000000004', 'star', 'Betelgeuse',
           'A red supergiant in Orion whose changing brightness is visible to the naked eye.',
           'https://upload.wikimedia.org/wikipedia/commons/5/56/Betelgeuse_star.jpg' FROM DUAL
    UNION ALL SELECT '20000000-0000-0000-0000-000000000005', 'black-hole', 'Sagittarius A*',
           'The supermassive black hole at the center of the Milky Way.',
           'https://upload.wikimedia.org/wikipedia/commons/4/4f/Black_hole_-_Messier_87_crop_max_res.jpg' FROM DUAL
) source ON (target.SOURCE_TYPE = source.SOURCE_TYPE AND target.SOURCE_ID = HEXTORAW(REPLACE(source.SOURCE_ID, '-', '')))
WHEN MATCHED THEN UPDATE SET
    target.NAME = source.NAME,
    target.DESCRIPTION = source.DESCRIPTION,
    target.IMAGE_URL = source.IMAGE_URL,
    target.UPDATED_AT = SYS_EXTRACT_UTC(SYSTIMESTAMP)
WHEN NOT MATCHED THEN INSERT (
    ID, SOURCE_ID, SOURCE_TYPE, NAME, DESCRIPTION, IMAGE_URL, UPDATED_AT
) VALUES (
    SYS_GUID(), HEXTORAW(REPLACE(source.SOURCE_ID, '-', '')), source.SOURCE_TYPE,
    source.NAME, source.DESCRIPTION, source.IMAGE_URL, SYS_EXTRACT_UTC(SYSTIMESTAMP)
);

COMMIT;
