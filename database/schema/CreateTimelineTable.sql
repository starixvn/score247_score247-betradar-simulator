-- Table: public."Timeline"

-- DROP TABLE public."Timeline";

CREATE TABLE public."Timeline"
(
    "Id" text COLLATE pg_catalog."default" NOT NULL,
    "MatchId" text COLLATE pg_catalog."default" NOT NULL,
    "Value" jsonb NOT NULL,
    "CreatedTime" timestamp with time zone,
    "ModifiedTime" timestamp with time zone,
    CONSTRAINT "Timeline_pkey" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Timeline"
    OWNER to postgres;