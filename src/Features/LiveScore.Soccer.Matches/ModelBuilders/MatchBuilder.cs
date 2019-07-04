namespace LiveScore.Soccer.Shared.ModelBuilders
{
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Soccer.Shared.Models;
    using LiveScore.Soccer.Dtos;

    public static class MatchBuilder
    {
        public static Match BuildMatch(Sport_Event sportEvent, Sport_Event_Status sportEventStatusDto, string region)
        {
            var match = new Match
            {
                Id = sportEvent.id,
                EventDate = sportEvent.scheduled,
                Teams = TeamBuilder.BuildTeams(sportEvent),
                League = LeagueBuilder.BuildLeague(sportEvent.tournament),
                LeagueRound = LeagueBuilder.BuildLeagueRound(sportEvent.tournament_round),
                MatchResult = BuildMatchResult(sportEventStatusDto),
                Region = region,
                Venue = BuildVenue(sportEvent.venue)
            };

            return match;
        }

        public static MatchResult BuildMatchResult(Sport_Event_Status sportEventStatusDto = null)
        {
            var matchResult = new MatchResult
            {
                EventStatus = MatchStatus.NotStarted,
                MatchStatus = MatchStatus.NotStarted,
                HomeScore = 0,
                AwayScore = 0,
                MatchTime = 0
            };

            if (sportEventStatusDto != null)
            {
                matchResult = new MatchResult
                {
                    EventStatus = Enumeration.FromValue<MatchStatus>(sportEventStatusDto.status),
                    HomeScore = sportEventStatusDto.home_score,
                    AwayScore = sportEventStatusDto.away_score,
                    WinnerId = sportEventStatusDto.winner_id
                };

                if (!string.IsNullOrWhiteSpace(sportEventStatusDto.aggregate_winner_id))
                {
                    matchResult.AggregateWinnerId = sportEventStatusDto.aggregate_winner_id;
                    matchResult.AggregateHomeScore = sportEventStatusDto.aggregate_home_score;
                    matchResult.AggregateAwayScore = sportEventStatusDto.aggregate_away_score;
                }

                if (!string.IsNullOrWhiteSpace(sportEventStatusDto.match_status))
                {
                    matchResult.MatchStatus = Enumeration.FromValue<MatchStatus>(sportEventStatusDto.match_status);
                }

                matchResult.Period = sportEventStatusDto.period;

                if (sportEventStatusDto.period_scores != null
                        && sportEventStatusDto.period_scores.Any())
                {
                    matchResult.MatchPeriods = sportEventStatusDto.period_scores.Select(
                        periodScore =>
                        new MatchPeriod
                        {
                            HomeScore = periodScore.home_score,
                            AwayScore = periodScore.away_score,
                            PeriodType = Enumeration.FromValue<PeriodType>(periodScore.type),
                            Number = periodScore.number
                        });

                    matchResult.MatchTime = sportEventStatusDto.clock == null
                        ? 0
                        : ParseMatchClock(sportEventStatusDto.clock.match_time);
                }

            }

            return matchResult;
        }

        public static Timeline BuildTimeLine(TimelineDto timelineDto)
        {
            var timeline = new Timeline();

            if (timelineDto != null)
            {
                timeline.Id = timelineDto.id;
                timeline.Type = timelineDto.type;
                timeline.Team = timelineDto.team;
                timeline.InjuryTimeAnnounced = timelineDto.injury_time_announced;
                timeline.PeriodType = timelineDto.period_type;
                if (string.IsNullOrWhiteSpace(timeline.PeriodType)
                    && !string.IsNullOrWhiteSpace(timelineDto.period_name))
                {
                    timeline.PeriodType = timelineDto.period_name.ToLowerInvariant().Replace(" ", "_").Trim();
                }
                timeline.Period = timelineDto.period;
                timeline.Time = timelineDto.time;
                timeline.HomeScore = timelineDto.home_score;
                timeline.AwayScore = timelineDto.away_score;
                timeline.StoppageTime = timelineDto.stoppage_time;
                timeline.Commentaries = timelineDto.commentaries?.Select(x => new Commentary { Text = x.text });

                timeline.MatchTime = timelineDto.match_time == 0
                    ? ParseMatchClock(timelineDto.match_clock)
                    : timelineDto.match_time;

                timeline.GoalScorer = timelineDto.goal_scorer == null
                    ? null
                    : new GoalScorer { Name = timelineDto.goal_scorer.name, Id = timelineDto.goal_scorer.id, Method = timelineDto.goal_scorer.method };

                timeline.Assist = timelineDto.assist == null
                    ? null
                    : new Player { Name = timelineDto.assist.name, Id = timelineDto.assist.id };

                timeline.Player = timelineDto.player == null
                    ? null
                    : new Player { Name = timelineDto.player.name, Id = timelineDto.player.id };

                SetPenaltyInfo(timelineDto, timeline);
            }

            return timeline;
        }

        private static void SetPenaltyInfo(TimelineDto timelineDto, Timeline timeline)
        {
            if (timelineDto.period_type == PeriodType.Penalties.Value)
            {
                var isScored = timelineDto.status == "scored";
                var player = timelineDto.player == null
                    ? null
                    : new Player { Name = timelineDto.player.name, Id = timelineDto.player.id };
                if (timelineDto.team == "home")
                {
                    timeline.IsHomeShootoutScored = isScored;
                    timeline.HomeShootoutPlayer = player;
                }
                else
                {
                    timeline.IsAwayShootoutScored = isScored;
                    timeline.AwayShootoutPlayer = player;
                }
                timeline.PenaltyStatus = timelineDto.status;
            }
        }

        public static Dictionary<string, MatchPushEvent> BuildPushEvents(MatchPushEventDto pushEventDto)
        {
            var matchId = pushEventDto.metadata.sport_event_id;
            var pushEvent = new MatchPushEvent
            {
                MatchResult = BuildMatchResult(pushEventDto.payload.sport_event_status),
                TimeLines = new List<Timeline> { BuildTimeLine(pushEventDto.payload.timeline) }
            };

            return new Dictionary<string, MatchPushEvent>
            {
                { matchId, pushEvent }
            };
        }

        public static Venue BuildVenue(VenueDto venueDto)
        {
            var venue = new Venue();

            if (venueDto != null)
            {
                venue.Name = venueDto.name;
                venue.Capacity = venueDto.capacity;
                venue.CityName = venueDto.city_name;
                venue.CountryName = venueDto.country_name;
                venue.CountryCode = venueDto.country_code;
            }

            return venue;
        }

        public static int ParseMatchClock(string matchClock)
        => string.IsNullOrWhiteSpace(matchClock)
                        ? 0
                        : int.Parse(matchClock.Split(':')[0]);

    }
}
