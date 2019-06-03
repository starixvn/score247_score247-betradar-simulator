ALTER TABLE public."Match" DROP CONSTRAINT "Match_PrimaryKey";

ALTER TABLE public."Match"
    ADD CONSTRAINT "Match_PrimaryKey" PRIMARY KEY ("Id", "Language")
;