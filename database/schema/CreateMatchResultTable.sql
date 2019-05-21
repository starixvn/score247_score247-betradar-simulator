-- Table: public."MatchResult"

-- DROP TABLE public."MatchResult";

CREATE TABLE public."MatchResult"
(
    "Value" json NOT NULL,
    "MatchId" text COLLATE pg_catalog."default" NOT NULL,
    "Id" SERIAL PRIMARY KEY
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."MatchResult"
    OWNER to postgres;