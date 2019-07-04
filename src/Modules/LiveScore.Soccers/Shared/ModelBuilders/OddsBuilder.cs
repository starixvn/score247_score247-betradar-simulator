namespace LiveScore.Soccers.Shared.ModelBuilders
{
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Features.Odds.Models;
    using LiveScore.Shared.Models;
    using LiveScore.Shared.Models.Dtos;

    public static class OddsBuilder
    {
        private const int OneXTwoBetTypeId = 1; //2, 3way
        private const string OneXTwoSportRadarBetTypeName = "3way";
        private const int OverUnderBeTypeId = 2; //3, total
        private const string OverUnderSportRadarBetTypeName = "total";
        private const int AsianHandicapBetTypeId = 3;// 5, asian_handicap
        private const string AsianHandicapSportRadarBetTypeName = "asian_handicap";
        private const int UnknownBetTypeId = 1000;

        public static IEnumerable<MatchOdds> BuildMatchOddsList(OddsScheduleDto oddsScheduleDto)
        {
            if (oddsScheduleDto == null || !oddsScheduleDto.sport_events.Any())
            {
                return Enumerable.Empty<MatchOdds>();
            }

            var matchOddsList = new List<MatchOdds>();

            foreach (var sportEvent in oddsScheduleDto.sport_events)
            {
                matchOddsList.Add(new MatchOdds
                {
                    MatchId = sportEvent.id,
                    BetTypeOddsList = BuildBetTypeOdds(sportEvent)
                });
            }

            return matchOddsList;
        }

        public static IEnumerable<BetTypeOdds> BuildBetTypeOdds(SportEvent sport_event)
        {
            var betTypeOddsList = new List<BetTypeOdds>();
            if (sport_event.markets.Any())
            {
                foreach (var market in sport_event.markets)
                {
                    var betType = GetBetType(market);

                    if(betType.Item1 == UnknownBetTypeId)
                    {
                        continue;
                    }

                    foreach (var book in market.books)
                    {
                        betTypeOddsList.Add(new BetTypeOdds
                        {
                            Id = betType.Item1,
                            Name = betType.Item2,
                            Bookmaker = new Bookmaker
                            {
                                Id = book.id,
                                Name = book.name
                            },
                            BetOptions = book.outcomes.Select(oc => new BetOptionOdds
                            {
                                Id = oc.odds_field_id,
                                LiveOdds = decimal.Parse(oc.odds ?? "0"),
                                OpeningOdds = decimal.Parse(oc.opening_odds ?? oc.odds ?? "0"),
                                Type = oc.type,
                                OddsTrend = Enumeration.FromValue<OddsTrend>(oc.odds_trend),
                                OptionValue = oc.handicap,
                                Name = GetOptionName(betType.Item1, oc.type)
                            })
                        });
                    }
                }
            }

            return betTypeOddsList;
        }

        private static (int, string) GetBetType(Market market)
        {
            switch (market.name)
            {
                case OneXTwoSportRadarBetTypeName:
                    return (OneXTwoBetTypeId, "1x2");

                case OverUnderSportRadarBetTypeName:
                    return (OverUnderBeTypeId, "OverUnder");

                case AsianHandicapSportRadarBetTypeName:
                    return (AsianHandicapBetTypeId, "Handicap");

                default:
                    return (UnknownBetTypeId, "Un-supported Bet Type");
            }
        }

        private static string GetOptionName(int betTypeId, string type)
        {
            switch (betTypeId)
            {
                case OneXTwoBetTypeId:
                    return type == "home"
                        ? "1"
                        : type == "away"
                            ? "2"
                            : "x";

                case OverUnderBeTypeId:
                    return type == "over"
                        ? "Over"
                        : "Under";

                default:
                    return type;
            }
        }
    }
}