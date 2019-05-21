-- Table: public."Timeline"

-- DROP TABLE public."Timeline";

CREATE TABLE public."Timeline"
(
    "Id" integer NOT NULL DEFAULT nextval('"Timeline_Id_seq"'::regclass),
    "Value" jsonb NOT NULL,
    "MatchId" text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "Timeline_pkey" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Timeline"
    OWNER to postgres;