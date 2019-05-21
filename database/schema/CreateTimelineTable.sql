-- Table: public."Timeline"

-- DROP TABLE public."Timeline";

CREATE TABLE public."Timeline"
(
    "Id" SERIAL PRIMARY KEY,
    "Value" jsonb NOT NULL,
    "MatchId" text COLLATE pg_catalog."default" NOT NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Timeline"
    OWNER to postgres;