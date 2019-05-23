namespace LiveScore.Soccers.Features.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Shared.SportRadarApi.Models;

    public class SportRadarMatchDataAccess : IMatchDataAccess
    {
        private const string Home = "home";
        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private const string FeatureKey = "MatchSchedule";

        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarMatchDataAccess(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language, TimeSpan timeZone)
        {
            var sportSetting = appSettings
                .DataProviderSettings
                .Sports.FirstOrDefault(sport => sport.Id == sportId)
                ?.EndPoints.FirstOrDefault(x => x.Name.Equals(FeatureKey));

            if (sportSetting == null)
            {
                throw new InvalidOperationException("Missing Sport Setting endpoints");
            }

            var regions = appSettings
                .DataProviderSettings
                .Sports.FirstOrDefault(sport => sport.Id == sportId)
                ?.Regions;

            var matches = new List<Match>();

            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();
            var today = DateTime.Now.ToUniversalTime();
            const int dayIncrementIndex = 1;

            for (var date = fromUtc.Date; date.Date <= toUtc.Date; date = date.AddDays(dayIncrementIndex))
            {

                if (date >= today.Date)
                {
                    await FetchDailyScheduleAll(language, sportSetting, regions, matches, date);
                }
                else
                {
                    //TODO fetch result
                    await FetchResults(language, sportSetting, regions, matches, date);
                }
            }

            return matches;
        }

        private async Task FetchResults(string language, EndPoint sportSetting, IEnumerable<Region> regions, List<Match> matches, DateTime date)
        {
            foreach (var endPoint in sportSetting.EndPointSettings.Where(x => x.IsPast))
            {
                var dailyResult = await FetchDailyResult(date, language, regions, endPoint);

                if (dailyResult.results != null && dailyResult.results.Any())
                {
                    dailyResult.results.ToList().ForEach(x => matches.Add(CreateMatch(x.sport_event, x.sport_event_status)));
                }
            }
        }

        private async Task FetchDailyScheduleAll(string language, EndPoint sportSetting, IEnumerable<Region> regions, List<Match> matches, DateTime date)
        {
            foreach (var endPoint in sportSetting.EndPointSettings.Where(x => !x.IsPast))
            {
                var dailySchedule = await FetchDailySchedule(date, language, regions, endPoint);

                if (dailySchedule.sport_events != null && dailySchedule.sport_events.Any())
                {
                    dailySchedule.sport_events.ToList().ForEach(x => matches.Add(CreateMatch(x)));
                }
            }
        }

        private async Task<DailyScheduleDto> FetchDailySchedule(DateTime date, string language, IEnumerable<Region> regions, EndPointSetting endPoint)
        {
            var finalDailySchedules = new DailyScheduleDto { generated_at = DateTime.Now.ToUniversalTime(), sport_events = new List<Sport_Event>() };

            foreach (var region in regions)
            {
                var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);
                var dailyResult = await restClient.ExecuteGetAsync<DailyScheduleDto>(formattedEndpoint);

                if (dailyResult.sport_events != null && dailyResult.sport_events.Any())
                {
                    finalDailySchedules.sport_events = finalDailySchedules.sport_events.Concat(dailyResult.sport_events);
                }
            }

            return finalDailySchedules;
        }

        public async Task<DailyResultDto> FetchDailyResult(DateTime date, string language, IEnumerable<Region> regions, EndPointSetting endPoint)
        {
            var finalDailyResults = new DailyResultDto { generated_at = DateTime.Now.ToUniversalTime(), results = new List<Result>() };

            foreach (var region in regions)
            {
                var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);
                var dailyResult = await restClient.ExecuteGetAsync<DailyResultDto>(formattedEndpoint);

                if (dailyResult.results != null && dailyResult.results.Any())
                {
                    finalDailyResults.results = finalDailyResults.results.Concat(dailyResult.results);
                }
            }

            return finalDailyResults;
        }

        private static Match CreateMatch(Sport_Event sportEvent, Sport_Event_Status sportEventStatusDto = null)
        {
            var match = new Match
            {
                Id = sportEvent.id,
                EventDate = sportEvent.scheduled,
                Teams = BuildTeams(sportEvent),
                League = BuildLeague(sportEvent.tournament),
                LeagueRound = BuildLeagueRound(sportEvent.tournament_round)
            };

            match.MatchResult = BuildMatchStatus(sportEventStatusDto);

            return match;
        }

        public static MatchResult BuildMatchStatus(Sport_Event_Status sportEventStatusDto = null)
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
                        : Int32.Parse(sportEventStatusDto.clock.match_time.Split(':')[0]);
                }

            }

            return matchResult;
        }

        private static LeagueRound BuildLeagueRound(Tournament_Round leagueRoundDto)
        {
            var leagueRound = new LeagueRound();

            if (leagueRoundDto != null)
            {
                leagueRound.Type = Enumeration.FromValue<LeagueRoundTypes>(leagueRoundDto.type);
                leagueRound.Name = leagueRoundDto.name;
                leagueRound.Number = leagueRoundDto.number;
                leagueRound.Phase = leagueRoundDto.phase;
                leagueRound.Group = leagueRoundDto.group;
            }

            return leagueRound;
        }

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

        public Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddResult(string matchId, IMatchResult matchResult)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines)
        {
            throw new NotImplementedException();
        }
    }
}
