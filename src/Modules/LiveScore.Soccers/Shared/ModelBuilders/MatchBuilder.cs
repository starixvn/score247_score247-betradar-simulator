namespace LiveScore.Soccers.Shared.ModelBuilders
{   
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Models;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Soccers.Shared.Dtos;

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
                    AwayScore = sportEventStatusDto.away_score
                };

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
                        : int.Parse(sportEventStatusDto.clock.match_time.Split(':')[0]);
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
                timeline.MatchTime = timelineDto.match_time;
                timeline.Team = timelineDto.team;
                timeline.InjuryTimeAnnounced = timelineDto.injury_time_announced;
                timeline.PeriodType = timelineDto.period_type;
                timeline.Period = timelineDto.period;
                timeline.Time = timelineDto.time;
                timeline.HomeScore = timelineDto.home_score;
                timeline.AwayScore = timelineDto.away_score;
                timeline.Commentaries = timelineDto.commentaries?.Select(x => new Commentary { Text = x.text });
                timeline.GoalScorer = new GoalScorer { Name = timelineDto.goal_scorer?.name, Id = timelineDto.goal_scorer?.id, Method = timelineDto.goal_scorer?.method  };
                timeline.Assist = new Player { Name = timelineDto.assist?.name, Id = timelineDto.assist?.id };
                timeline.Player = new Player { Name = timelineDto.player?.name, Id = timelineDto.player?.id };
            }

            return timeline;
        }

        public static Dictionary<string, MatchPushEvent> BuildPushEvents(MatchPushEventDto pushEventDto)
        {
            var pushEvents = new Dictionary<string, MatchPushEvent>();
            var matchId = pushEventDto.metadata.sport_event_id;
            var pushEvent = new MatchPushEvent
            {
                MatchResult = BuildMatchResult(pushEventDto.payload.sport_event_status),
                TimeLines = new List<Timeline> { BuildTimeLine(pushEventDto.payload.timeline) }
            };

            pushEvents.Add(matchId, pushEvent);

            return pushEvents;
        }

        public static Venue BuildVenue(VenueDto venueDto)
        {
            return new Venue
            {
                Name = venueDto.name,
                Capacity = venueDto.capacity,
                CityName = venueDto.city_name,
                CountryName = venueDto.country_name,
                CountryCode = venueDto.country_code
            };
        }
    }
}
