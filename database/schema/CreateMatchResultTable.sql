-- Table: public."MatchResult"

-- DROP TABLE public."MatchResult";

CREATE TABLE public."MatchResult"
(
    "Id" text COLLATE pg_catalog."default" NOT NULL,
    "MatchId" text COLLATE pg_catalog."default" NOT NULL,
    "Value" jsonb NOT NULL,
    "CreatedTime" timestamp with time zone,
    "ModifiedTime" timestamp with time zone,
    CONSTRAINT "MatchResult_pkey" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."MatchResult"
    OWNER to postgres;