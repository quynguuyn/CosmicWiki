-- Run as COSMICWIKI_STORIES after the Stories migrations.
-- Idempotent content seed for the Stories page.

MERGE INTO STORIES target
USING (
    SELECT
        '10000000-0000-0000-0000-000000000001' ID,
        'Space Exploration' CATEGORY,
        '20th century' ERA,
        'STORY-APOLLO-11' CODE,
        'Apollo 11: The First Footprints' TITLE,
        'apollo-11-first-footprints' SLUG,
        'A documentary account of the mission that carried humanity to the surface of another world.' DESCRIPTION,
        'Apollo 11 launched from Kennedy Space Center on 16 July 1969. Four days later, Neil Armstrong and Buzz Aldrin descended to the lunar surface while Michael Collins orbited above. Their observations, samples, and photographs remain one of the most important scientific archives ever created.' CONTENT,
        'CosmicWiki Editorial' AUTHOR,
        8 READ_TIME_MINUTES,
        'https://images-assets.nasa.gov/image/as11-40-5875/as11-40-5875~large.jpg' IMAGE_URL,
        'Apollo 11 lunar module on the Moon' IMAGE_ALT,
        1 STATUS
    FROM DUAL
) source ON (target.CODE = source.CODE)
WHEN MATCHED THEN UPDATE SET
    target.CATEGORY = source.CATEGORY,
    target.ERA = source.ERA,
    target.TITLE = source.TITLE,
    target.SLUG = source.SLUG,
    target.DESCRIPTION = source.DESCRIPTION,
    target.CONTENT = source.CONTENT,
    target.AUTHOR = source.AUTHOR,
    target.READ_TIME_MINUTES = source.READ_TIME_MINUTES,
    target.IMAGE_URL = source.IMAGE_URL,
    target.IMAGE_ALT = source.IMAGE_ALT,
    target.STATUS = source.STATUS,
    target.UPDATED_AT_UTC = SYS_EXTRACT_UTC(SYSTIMESTAMP)
WHEN NOT MATCHED THEN INSERT (
    ID, CATEGORY, ERA, CODE, TITLE, SLUG, DESCRIPTION, CONTENT, AUTHOR,
    READ_TIME_MINUTES, IMAGE_URL, IMAGE_ALT, STATUS, CREATED_AT_UTC, UPDATED_AT_UTC
) VALUES (
    HEXTORAW(REPLACE(source.ID, '-', '')), source.CATEGORY, source.ERA, source.CODE,
    source.TITLE, source.SLUG, source.DESCRIPTION, source.CONTENT, source.AUTHOR,
    source.READ_TIME_MINUTES, source.IMAGE_URL, source.IMAGE_ALT, source.STATUS,
    SYS_EXTRACT_UTC(SYSTIMESTAMP), SYS_EXTRACT_UTC(SYSTIMESTAMP)
);

MERGE INTO STORIES target
USING (
    SELECT
        '10000000-0000-0000-0000-000000000002' ID,
        'Astronomy' CATEGORY,
        '17th century' ERA,
        'STORY-GALILEO-MOONS' CODE,
        'Galileo and the Moons of Jupiter' TITLE,
        'galileo-and-the-moons-of-jupiter' SLUG,
        'How four points of light changed the human understanding of the Solar System.' DESCRIPTION,
        'In January 1610 Galileo Galilei turned his telescope toward Jupiter and recorded four moving lights. Night after night they changed position, proving that not every celestial body orbited Earth. The observation helped move astronomy from inherited certainty toward evidence and repeatable measurement.' CONTENT,
        'CosmicWiki Editorial' AUTHOR,
        6 READ_TIME_MINUTES,
        'https://upload.wikimedia.org/wikipedia/commons/1/10/Galileo%27s_sketches_of_the_moon_and_the_moons_of_Jupiter.jpg' IMAGE_URL,
        'Galileo sketches of the Moon and Jupiter moons' IMAGE_ALT,
        1 STATUS
    FROM DUAL
) source ON (target.CODE = source.CODE)
WHEN MATCHED THEN UPDATE SET
    target.CATEGORY = source.CATEGORY, target.ERA = source.ERA, target.TITLE = source.TITLE,
    target.SLUG = source.SLUG, target.DESCRIPTION = source.DESCRIPTION, target.CONTENT = source.CONTENT,
    target.AUTHOR = source.AUTHOR, target.READ_TIME_MINUTES = source.READ_TIME_MINUTES,
    target.IMAGE_URL = source.IMAGE_URL, target.IMAGE_ALT = source.IMAGE_ALT, target.STATUS = source.STATUS,
    target.UPDATED_AT_UTC = SYS_EXTRACT_UTC(SYSTIMESTAMP)
WHEN NOT MATCHED THEN INSERT (
    ID, CATEGORY, ERA, CODE, TITLE, SLUG, DESCRIPTION, CONTENT, AUTHOR,
    READ_TIME_MINUTES, IMAGE_URL, IMAGE_ALT, STATUS, CREATED_AT_UTC, UPDATED_AT_UTC
) VALUES (
    HEXTORAW(REPLACE(source.ID, '-', '')), source.CATEGORY, source.ERA, source.CODE,
    source.TITLE, source.SLUG, source.DESCRIPTION, source.CONTENT, source.AUTHOR,
    source.READ_TIME_MINUTES, source.IMAGE_URL, source.IMAGE_ALT, source.STATUS,
    SYS_EXTRACT_UTC(SYSTIMESTAMP), SYS_EXTRACT_UTC(SYSTIMESTAMP)
);

COMMIT;
