namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Models;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Soccers.Features.Matches.DataProviders;

    public class MatchDataAccessImpl : MatchDataAccess
    {
        private const string Home = "home";
        private readonly IMatchApi matchApi;

        public MatchDataAccessImpl(IMatchApi matchApi)
        {
            this.matchApi = matchApi;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language)
        {
            var dailySchedule = await matchApi.GetDailySchedules(
                sportId,
                from,
                to,
                language);

            var matches = new List<Match>();

            foreach (var matchDto in dailySchedule.results)
            {
                matches.Add(CreateMatch(matchDto));
            }

            return matches;
        }

        private static Match CreateMatch(Result matchDto)
        {
            var sportEventDto = matchDto.sport_event;
            var sportEventStatusDto = matchDto.sport_event_status;
            var matchResult = BuildMatchStatus(sportEventStatusDto);

            var match = new Match
            {
                Id = sportEventDto.id,
                EventDate = sportEventDto.scheduled,
                MatchResult = matchResult,
                Teams = BuildTeams(sportEventDto),
                League = BuildLeague(matchDto.sport_event.tournament),
                LeagueRound = BuildLeagueRound(matchDto.sport_event.tournament_round)
            };

            return match;
        }

        private static MatchResult BuildMatchStatus(Sport_Event_Status sportEventStatusDto)
        {
            var matchResult = new MatchResult
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

                matchResult.MatchTime = sportEventStatusDto.clock?.match_time;
            }

            return matchResult;
        }

        private static LeagueRound BuildLeagueRound(Tournament_Round leagueRoundDto)
            => new LeagueRound
            {
                Type = Enumeration.FromValue<LeagueRoundTypes>(leagueRoundDto.type),
                Name = leagueRoundDto.name,
                Number = leagueRoundDto.number,
                Phase = leagueRoundDto.phase
            };

        private static League BuildLeague(
            Tournament leagueDto)
            => new League
            {
                Id = leagueDto?.id,
                Name = leagueDto?.name,
                Category = new LeagueCategory
                {
                    Id = leagueDto?.category?.id,
                    Name = leagueDto?.category?.name,
                    CountryCode = leagueDto?.category?.country_code
                }
            };

        private static List<LiveScore.Features.Teams.Models.Team> BuildTeams(Sport_Event sportEventDto)
        {
            var teams = new List<LiveScore.Features.Teams.Models.Team>();

            foreach (var competitor in sportEventDto.competitors)
            {
                teams.Add(new LiveScore.Features.Teams.Models.Team
                {
                    Id = competitor.id,
                    Country = competitor.country,
                    CountryCode = competitor.country_code,
                    Name = competitor.name,
                    Abbreviation = competitor.abbreviation,
                    IsHome = string.Compare(competitor.qualifier, Home, true, CultureInfo.InvariantCulture) == 0
                });
            }

            return teams;
        }
    }
}