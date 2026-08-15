-- Schema version 1. Applied by DatabaseSync.Sync(); never run manually.

CREATE TABLE IF NOT EXISTS questions (
    id                 INTEGER PRIMARY KEY,
    olympiad           TEXT    NOT NULL,
    stage              TEXT    NOT NULL,
    year               INTEGER,
    difficulty         INTEGER,
    source             TEXT,
    source_url         TEXT,
    source_raw         TEXT,
    explanation_source TEXT,
    type               TEXT    NOT NULL,
    content            TEXT    NOT NULL DEFAULT '[]',
    content_cpp        TEXT,
    options            TEXT    NOT NULL DEFAULT '[]',
    match_options      TEXT,
    correct_answer     TEXT    NOT NULL DEFAULT '[]',
    category           TEXT    NOT NULL DEFAULT '[]',
    algorithms         TEXT    NOT NULL DEFAULT '[]',
    explanation        TEXT,
    points             INTEGER NOT NULL DEFAULT 1,
    partial_credit     INTEGER NOT NULL DEFAULT 0,
    content_hash       TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_questions_stage ON questions (stage);
CREATE INDEX IF NOT EXISTS idx_questions_year  ON questions (year);
CREATE INDEX IF NOT EXISTS idx_questions_type  ON questions (type);
