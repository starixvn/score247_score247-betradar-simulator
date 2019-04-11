namespace LiveScore.Soccers.Features.Leagues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues;
    using LiveScore.Features.Leagues.Models;

    public class LeagueDataAccessImpl : LeagueDataAccess
    {
        public async Task<IEnumerable<ILeague>> GetLeagues(int sportId, DateTime date)
        {
            ////var dailySchedule = await leagueApi.GetDailySchedules(
            ////    "soccer",
            ////    "eu",
            ////    "en",
            ////    DateTime.Now.ToString(),
            ////    "key");

            return await Task.FromResult(Enumerable.Empty<ILeague>());

            ////var leagueDtoGroups = dailySchedule
            ////    .results
            ////    .Where(result => result?.sport_event?.tournament != null)
            ////    .GroupBy(result => result.sport_event.tournament.id);

            ////foreach (var leagueDtoGroup in leagueDtoGroups)
            ////{
            ////    var matchByLeagueDtos = leagueDtoGroup.ToList();
            ////    var leagueDto = matchByLeagueDtos.FirstOrDefault().sport_event.tournament;
            ////    var leagueCategoryDto = leagueDto.category;

            ////    var leagueRoundGroupDtos = matchByLeagueDtos
            ////        .Where(result => result?.sport_event?.tournament_round != null)
            ////        .GroupBy(result =>
            ////        string.Join(
            ////            "_",
            ////            result.sport_event.tournament_round.type,
            ////            result.sport_event.tournament_round.number));

            ////    var leagueRounds = BuildLeagueRound(matchByLeagueDtos, leagueRoundGroupDtos);

            ////    var league = new League
            ////    {
            ////        Id = leagueDto.id,
            ////        Name = leagueDto.name,
            ////        Category = new LeagueCategory
            ////        {
            ////            Id = leagueCategoryDto?.id,
            ////            Name = leagueCategoryDto?.name
            ////        },
            ////        Rounds = leagueRounds
            ////    };

            ////    leagues.Add(league);
            ////}

            ////return leagues;
        }

        ////private static List<LeagueRound> BuildLeagueRound(
        ////    List<Result> matchByLeagueDtos,
        ////    IEnumerable<IGrouping<string, Result>> leagueRoundGroupDtos)
        ////{
        ////    var leagueRounds = new List<LeagueRound>();

        ////    foreach (var leagueRoundGroupDto in leagueRoundGroupDtos)
        ////    {
        ////        var leagueRoundDto = matchByLeagueDtos.FirstOrDefault().sport_event.tournament_round;

        ////        var matches = BuildMatches(matchByLeagueDtos);

        ////        var leagueRound = new LeagueRound
        ////        {
        ////            Type = Enumeration.FromValue<LeagueRoundTypes>(leagueRoundDto.type),
        ////            Name = leagueRoundDto.name,
        ////            Number = leagueRoundDto.number,
        ////            Matches = matches
        ////        };

        ////        leagueRounds.Add(leagueRound);
        ////    }

        ////    return leagueRounds;
        ////}

        ////private static List<Match> BuildMatches(List<Result> matchByLeagueDtos)
        ////{
        ////    var matches = new List<Match>();

        ////    foreach (var matchDto in matchByLeagueDtos)
        ////    {
        ////        var sportEventDto = matchDto.sport_event;
        ////        var sportEventStatusDto = matchDto.sport_event_status;
        ////        var teams = new List<Teams.Models.Team>();

        ////        foreach (var competitor in sportEventDto.competitors)
        ////        {
        ////            teams.Add(new Teams.Models.Team
        ////            {
        ////                Id = competitor.id,
        ////                Country = competitor.country,
        ////                CountryCode = competitor.country_code,
        ////                Name = competitor.name,
        ////                Abbreviation = competitor.abbreviation,
        ////                IsHome = string.Compare(competitor.qualifier, "home", true) == 0
        ////            });
        ////        }

        ////        matches.Add(new Match
        ////        {
        ////            Id = sportEventDto.id,
        ////            EventDate = sportEventDto.scheduled,
        ////            MatchResult = new MatchResult
        ////            {
        ////                Status = Enumeration.FromValue<MatchStatus>(sportEventStatusDto.status),
        ////                HomeScores = new List<int> { sportEventStatusDto.home_score },
        ////                AwayScores = new List<int> { sportEventStatusDto.away_score },
        ////            },
        ////            Teams = teams
        ////        });
        ////    }

        ////    return matches;
        ////}
    }
}