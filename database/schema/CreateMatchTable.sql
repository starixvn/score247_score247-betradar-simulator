-- Table: public."Match"

-- DROP TABLE public."Match";

CREATE TABLE public."Match"
(
    "Id" text COLLATE pg_catalog."default" NOT NULL,
    "Value" jsonb NOT NULL,
    "Language" text COLLATE pg_catalog."default" NOT NULL DEFAULT 'en_US'::text,
    "SportId" integer NOT NULL DEFAULT 1,
    "LeagueId" text COLLATE pg_catalog."default" NOT NULL DEFAULT ''::text,
    "EventDate" timestamp with time zone,
    "CreatedTime" timestamp with time zone,
    "ModifiedTime" timestamp with time zone,
    "Region" text COLLATE pg_catalog."default",
    CONSTRAINT "Match_PrimaryKey" PRIMARY KEY ("Id", "Language")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Match"
    OWNER to postgres;