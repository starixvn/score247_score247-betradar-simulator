using LiveScore.SportRadar.Dtos;
using System.Collections.Generic;

namespace LiveScore.Soccers.UnitTests.Shared
{
    public static class StubTimelines
    {
        public static MatchTimelineDto StubMatchTimeLineDto()
        {
            return new MatchTimelineDto
            {
                timeline = new List<TimelineDto>
                {
                    new TimelineDto{ id = 539286726, type = "match_started", commentaries = new List<CommentaryDto>{ new CommentaryDto { text = "The first-half is now underway." } } },
                    new TimelineDto{ id = 539286728, type = "period_start", period = 1, period_name = "1st half", period_type = "regular_period" },
                    StubScoreChangeTimeline(),
                    StubYellowCardTimeline(),
                    StubScoreChangePenalty(),
                    StubYellowRedCardTimeline(),
                    StubRedCardTimeline(),
                    new TimelineDto{ id = 539286728, type = "match_ended", match_clock = "90:00" }
                }
            };
        }

        public static TimelineDto StubScoreChangeTimeline()
        {
            return new TimelineDto
            {
                id = 539289582,
                type = "score_change",
                period = 1,
                period_type = "regular_period",
                match_time = 5,
                home_score = 0,
                away_score = 1,
                team = "away",
                goal_scorer = new Goal_Scorer { id = "sr:player:331209", name = "Silva, Bernardo" },
                assist = new Assist { id = "sr:player:17094", type = "primary", name = "Aguero, Sergio" },
                commentaries = new List<CommentaryDto>
                {
                    new CommentaryDto{ text = "Bernardo Silva scores to make it 0-1 at Craven Cottage."},
                    new CommentaryDto{ text = "Sergio Aguero instrumental with a fine assist."},
                }
            };

        }

        public static TimelineDto StubScoreChangePenalty()
        {
            return new TimelineDto
            {
                id = 539289582,
                type = "score_change",
                period = 2,
                period_type = "regular_period",
                match_time = 90,
                home_score = 0,
                away_score = 1,
                team = "away",
                stoppage_time = "4",
                goal_scorer = new Goal_Scorer { id = "sr:player:331209", name = "Silva, Bernardo", method = "penalty" },
                commentaries = new List<CommentaryDto>
                {
                    new CommentaryDto{ text = "Goal! The away team extend their lead to 1-3 thanks to a penalty from Marcus Rashford."}
                }
            };

        }

        public static TimelineDto StubYellowCardTimeline()
        {
            return new TimelineDto
            {
                id = 539289582,
                type = "yellow_card",
                period = 2,
                period_type = "regular_period",
                match_time = 67,
                team = "home",
                player = new TimelinePlayer { id = "sr:player:125558", name = "Christie, Cyrus" },
                commentaries = new List<CommentaryDto>
                {
                    new CommentaryDto{ text = "At Craven Cottage, Cyrus Christie has been yellow-carded for the home team."}
                }
            };

        }

        public static TimelineDto StubYellowRedCardTimeline()
        {
            return new TimelineDto
            {
                id = 539289582,
                type = "yellow_red_card",
                period = 2,
                period_type = "regular_period",
                match_time = 67,
                team = "home",
                player = new TimelinePlayer { id = "sr:player:125558", name = "Christie, Cyrus" },
                commentaries = new List<CommentaryDto>
                {
                    new CommentaryDto{ text = "At Craven Cottage, Cyrus Christie has been red_card for the home team."}
                }
            };

        }

        public static TimelineDto StubRedCardTimeline()
        {
            return new TimelineDto
            {
                id = 539289582,
                type = "red_card",
                period = 2,
                period_type = "regular_period",
                match_time = 67,
                team = "home",
                player = new TimelinePlayer { id = "sr:player:125558", name = "Christie, Cyrus" },
                commentaries = new List<CommentaryDto>
                {
                    new CommentaryDto{ text = "At Craven Cottage, Cyrus Christie has been red_card for the home team."}
                }
            };

        }
    }
}
