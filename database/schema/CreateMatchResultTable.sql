-- Table: public."MatchResult"

-- DROP TABLE public."MatchResult";

CREATE TABLE public."MatchResult"
(
    "Value" json NOT NULL,
    "MatchId" text COLLATE pg_catalog."default" NOT NULL,
    "Id" integer NOT NULL DEFAULT nextval('"MatchResult_Id_seq"'::regclass),
    CONSTRAINT "MatchResult_pkey" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."MatchResult"
    OWNER to postgres;