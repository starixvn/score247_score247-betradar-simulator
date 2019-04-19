namespace LiveScore.Basketballs.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Features.Teams.Models;
    using Newtonsoft.Json;

    public class MatchDataAccessImpl : MatchDataAccess
    {
        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language)
        {
            return await Task.FromResult(new List<IMatch>
            {
                new Match
                {
                    EventDate = to,
                    Id = "test",
                    League = JsonConvert.DeserializeObject<League>(
                            "{\"order\": 0," +
                          "\"flag\": null," +
                          "\"category\": {" +
                            "\"countryCode\": null," +
                            "\"id\": \"sr:category:393\"," +
                            "\"name\": \"International Clubs\"" +
                          "}," +
                          "\"rounds\": null," +
                          "\"teams\": null," +
                          "\"id\": \"sr:tournament:668\"," +
                          "\"name\": \"AFC Cup\"}"),
                    LeagueRound = JsonConvert.DeserializeObject<LeagueRound>(
                        "{\"type\": {" +
                        "\"displayName\": \"Group\"," +
                        "\"value\": \"group\"" +
                        "}," +
                        "\"name\": null," +
                        "\"number\": 4," +
                        "\"phase\": null," +
                        "\"matches\": null}"),
                    Teams = JsonConvert.DeserializeObject<List<Team>>(
                        "[{" +
                           "\"country\": \"Philippines\"," +
                            "\"countryCode\": \"PHL\"," +
                            "\"flag\": null," +
                            "\"isHome\": true," +
                            "\"players\": null," +
                            "\"statistic\": null," +
                            "\"coach\": null," +
                            "\"formation\": null," +
                            "\"abbreviation\": \"KAY\"," +
                            "\"substitutions\": null," +
                            "\"id\": \"sr:competitor:142194\"," +
                            "\"name\": \"Kaya FC-Iloilo\"" +
                          "}," +
                          "{" +
                            "\"country\": \"Indonesia\"," +
                            "\"countryCode\": \"IDN\"," +
                            "\"flag\": null," +
                            "\"isHome\": false," +
                            "\"players\": null," +
                            "\"statistic\": null," +
                            "\"coach\": null," +
                            "\"formation\": null," +
                            "\"abbreviation\": \"MAK\"," +
                            "\"substitutions\": null," +
                            "\"id\": \"sr:competitor:135864\"," +
                            "\"name\": \"PSM Makassar\"" +
                          "}" +
                        "]"),
                    MatchResult = new MatchResult
                    {
                        MatchStatus = new MatchStatus("1st_half", "FirstHaft"),
                        EventStatus = new MatchStatus("live", "Live"),
                        Period = 1,
                        MatchTime = "29:17",
                        HomeScore = 1,
                        AwayScore = 0,
                        MatchPeriods = JsonConvert.DeserializeObject<List<MatchPeriod>>(
                            "[" +
                            "{" +
                              "\"homeScore\": 1," +
                              "\"awayScore\": 0," +
                              "\"periodType\": {" +
                                "\"displayName\": \"RegularPeriod\"," +
                                "\"value\": \"regular_period\"" +
                              "}," +
                              "\"number\": 1" +
                            "}]")
                    }
                },
            });
        }
    }
}