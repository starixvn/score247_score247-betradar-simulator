namespace LiveScore.Soccers.UnitTests.Shared
{
    using System.Collections.Generic;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Database.Entities;
    using Newtonsoft.Json;

    public static class StubTimeline
    {
        public static MatchTimelineEntity StubScoreChangeTimelineEntity(string matchId)
            => new MatchTimelineEntity
            {
                MatchId = matchId,
                Value = JsonConvert.SerializeObject(StubScoreChangeTimeline())
            };

        public static Timeline StubScoreChangeTimeline()
        => new Timeline
        {
            Id = 539289582,
            Type = "score_change",
            Period = 1,
            PeriodType = "regular_period",
            MatchTime = 5,
            HomeScore = 0,
            AwayScore = 1,
            Team = "away",
            GoalScorer = new GoalScorer { Id = "sr:player:331209", Name = "Silva, Bernardo" },
            Assist = new Player { Id = "sr:player:17094", Name = "Aguero, Sergio" },
            Commentaries = new List<Commentary>
                {
                    new Commentary{ Text = "Bernardo Silva scores to make it 0-1 at Craven Cottage."},
                    new Commentary{ Text = "Sergio Aguero instrumental with a fine assist."},
                }
        };

        public static MatchTimelineEntity StubScoreChangePenaltyEntity(string matchId)
            => new MatchTimelineEntity
            {
                MatchId = matchId,
                Value = JsonConvert.SerializeObject(StubScoreChangePenalty())
            };

        public static Timeline StubScoreChangePenalty()
        => new Timeline
        {
            Id = 539289582,
            Type = "score_change",
            Period = 2,
            PeriodType = "regular_period",
            MatchTime = 90,
            HomeScore = 0,
            AwayScore = 1,
            Team = "away",
            StoppageTime = "4",
            GoalScorer = new GoalScorer { Id = "sr:player:331209", Name = "Silva, Bernardo", Method = "penalty" },
            Commentaries = new List<Commentary>
            {
                new Commentary{ Text = "Goal! The away team extend their lead to 1-3 thanks to a penalty from Marcus Rashford."}
            }
        };

        public static MatchTimelineEntity StubSubstitutionEntity(string matchId)
            => new MatchTimelineEntity
            {
                MatchId = matchId,
                Value = JsonConvert.SerializeObject(StubSubstitution())
            };

        public static Timeline StubSubstitution()
        => new Timeline
        {
            Id = 539289582,
            Type = EventTypes.Substitution.Value,
            Period = 2,
            PeriodType = "regular_period",
            MatchTime = 55,
            Team = "away",
            PlayerIn = new Player { Id = "sr:player:17094", Name = "Aguero, Sergio" },
            PlayerOut = new Player { Id = "sr:player:17095", Name = "Out, Sergio" },
            Commentaries = new List<Commentary>
            {
                new Commentary{ Text = "Substitution"}
            }
        };


    }
}
