-- Table: public."Commentary"

-- DROP TABLE public."Commentary";

CREATE TABLE public."Commentary"
(
    "Value" json NOT NULL,  
    "MatchId" text COLLATE pg_catalog."default" NOT NULL,
    "Language" text COLLATE pg_catalog."default" NOT NULL,
    "Id" text COLLATE pg_catalog."default" NOT NULL,
    "CreatedTime" timestamp with time zone NOT NULL,
    "ModifiedTime" timestamp with time zone,
    CONSTRAINT "Commentary_pkey" PRIMARY KEY ("Id", "Language")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Commentary"
    OWNER to postgres;