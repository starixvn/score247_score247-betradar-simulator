namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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

        public async Task<IEnumerable<Match>> GetMatches(int sportId, DateTime date, string language)
        {
            var dailySchedule = await matchApi.GetDailySchedules(
                sportId,
                date,
                language);

            var matches = new List<Match>();

            foreach (var matchDto in dailySchedule.results)
            {
                matches.Add(CreateMatch(matchDto));
            }

            return matches;
        }

        private static Match CreateMatch(
            Result matchDto)
        {
            var sportEventDto = matchDto.sport_event;
            var sportEventStatusDto = matchDto.sport_event_status;

            return new Match
            {
                Id = sportEventDto.id,
                EventDate = sportEventDto.scheduled,
                MatchResult = new MatchResult
                {
                    Status = Enumeration.FromValue<MatchStatus>(sportEventStatusDto.status),
                    HomeScores = new List<int> { sportEventStatusDto.home_score },
                    AwayScores = new List<int> { sportEventStatusDto.away_score },
                },
                Teams = BuildTeams(sportEventDto),
                League = BuildLeague(matchDto.sport_event.tournament),
                LeagueRound = BuildLeagueRound(matchDto.sport_event.tournament_round)
            };
        }

        private static LeagueRound BuildLeagueRound(Tournament_Round leagueRoundDto)
        {
            return new LeagueRound
            {
                Type = Enumeration.FromValue<LeagueRoundTypes>(leagueRoundDto.type),
                Name = leagueRoundDto.name,
                Number = leagueRoundDto.number,
                Phase = leagueRoundDto.phase
            };
        }

        private static League BuildLeague(
            Tournament leagueDto)
        {
            var leagueCategoryDto = leagueDto.category;

            return new League
            {
                Id = leagueDto.id,
                Name = leagueDto.name,
                Category = new LeagueCategory
                {
                    Id = leagueCategoryDto?.id,
                    Name = leagueCategoryDto?.name,
                    CountryCode = leagueCategoryDto?.country_code
                }
            };
        }

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