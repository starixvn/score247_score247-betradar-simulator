namespace LiveScore.Soccers.Jobs.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Entity.Entities;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Jobs.Features.Matches;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Shared.SportRadarApi.Models;
    using Newtonsoft.Json;

    public class MatchJobDataAccessImpl : MatchJobDataAccess
    {

        private const string Home = "home";
        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private const string FeatureKey = "MatchSchedule";

        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public MatchJobDataAccessImpl(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public async Task<IEnumerable<MatchEntity>> GetDailySchedule(int sportId, Region region, DateTime date, string language)
        {
            var sportSetting = appSettings
                .DataProviderSettings
                .Sports.FirstOrDefault(sport => sport.Id == sportId)
                ?.EndPoints.FirstOrDefault(x => x.Name.Equals(FeatureKey));

            if (sportSetting == null)
            {
                throw new InvalidOperationException("Missing Sport Setting endpoints");
            }

            var dailyScheduleDtoSum = new DailyResultDto { generated_at = DateTime.Now.ToUniversalTime(), results = new List<Result>() };

            var scheduleDate = date.ToUniversalTime();

            var matchEntities = new List<MatchEntity>();

            foreach (var endPoint in sportSetting.EndPointSettings)
            {
                var dailyResult = await FetchMatches(date, language, region, endPoint);

                if (dailyResult.sport_events != null
                        && dailyResult.sport_events.Any())
                {
                    var entities = MapToEntities(language, dailyResult);

                    matchEntities.AddRange(entities);
                }
            }

            return matchEntities;
        }

        private async Task<DailyScheduleDto> FetchMatches(DateTime date, string language, Region region, EndPointSetting endPoint)
        {
            var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);

            var dailyResult = await restClient.ExecuteGetAsync<DailyScheduleDto>(formattedEndpoint);

            return dailyResult;
        }

        private static IEnumerable<MatchEntity> MapToEntities(string language, DailyScheduleDto dailyResult)
        {
            var matchEntities = new List<MatchEntity>();

            foreach (var matchDto in dailyResult.sport_events)
            {
                var match = CreateMatch(matchDto);

                var matchEntity = new MatchEntity
                {
                    Id = match.Id,
                    EventDate = match.EventDate,
                    SeasonId = string.Empty,
                    Value = JsonConvert.SerializeObject(match),
                    Language = language
                };

                matchEntities.Add(matchEntity);
            }

            return matchEntities;
        }

        private static Match CreateMatch(Sport_Event sportEvent)
        {
            var match = new Match
            {
                Id = sportEvent.id,
                EventDate = sportEvent.scheduled,
                MatchResult = BuildMatchStatus(),
                Teams = BuildTeams(sportEvent),
                League = BuildLeague(sportEvent.tournament),
                LeagueRound = BuildLeagueRound(sportEvent.tournament_round)
            };

            return match;
        }

        private static MatchResult BuildMatchStatus()
        {
            var matchResult = new MatchResult
            {
                EventStatus = MatchStatus.NotStarted,
                MatchStatus = MatchStatus.NotStarted,
                HomeScore = 0,
                AwayScore = 0,
                MatchTime = "0"
            };

            return matchResult;
        }

        private static LeagueRound BuildLeagueRound(Tournament_Round leagueRoundDto)
            => new LeagueRound
            {
                Type = Enumeration.FromValue<LeagueRoundTypes>(leagueRoundDto.type),
                Name = leagueRoundDto.name,
                Number = leagueRoundDto.number,
                Phase = leagueRoundDto.phase,
                Group = leagueRoundDto.group
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

            return teams.OrderBy(team => team.IsHome ? 0 : 1).ToList();
        }
    }
}
