namespace LiveScore.Soccers.Shared.ModelBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Features.Odds.Models;
    using LiveScore.Providers.SportRadar.Dtos;
    using LiveScore.Soccer.Odds.AppResources;

    public static class SportRadarOddsBuilder
    {
        private const string UnSupportedBetTypeName = "Un-supported Bet Type";
        private const int OneXTwoBetTypeId = 1; //2, 3way
        private const string OneXTwoSportRadarBetTypeName = "3way";
        private const int OverUnderBeTypeId = 2; //3, total
        private const string OverUnderSportRadarBetTypeName = "total";
        private const int AsianHandicapBetTypeId = 3;// 5, asian_handicap
        private const string AsianHandicapSportRadarBetTypeName = "asian_handicap";
        private const int UnknownBetTypeId = 1000;
        private const string home = "home";
        private const string away = "away";
        private const string one = "1";
        private const string two = "2";
        private const string x = "x";
        private const string over = "over";
        private const int numOfRounding = 2;

        public static IEnumerable<MatchOdds> BuildMatchOddsList(OddsScheduleDto oddsScheduleDto)
        {
            if (oddsScheduleDto == null
                || oddsScheduleDto.sport_events == null
                || !oddsScheduleDto.sport_events.Any())
            {
                return Enumerable.Empty<MatchOdds>();
            }

            var matchOddsList = new List<MatchOdds>();

            foreach (var sportEvent in oddsScheduleDto.sport_events)
            {
                matchOddsList.Add(new MatchOdds
                {
                    MatchId = sportEvent.id,
                    BetTypeOddsList = BuildBetTypeOddsList(sportEvent),
                    LastUpdated = sportEvent.markets_last_updated
                });
            }

            return matchOddsList;
        }

        public static IEnumerable<BetTypeOdds> BuildBetTypeOddsList(SportEvent sport_event)
        {
            var betTypeOddsList = new List<BetTypeOdds>();
            if (sport_event.markets.Any())
            {
                foreach (var market in sport_event.markets)
                {
                    var betType = GetBetType(market);

                    if (betType.Item1 == UnknownBetTypeId)
                    {
                        continue;
                    }

                    foreach (var book in market.books)
                    {
                        betTypeOddsList.Add(BuildBetTypeOdds(betType, book));
                    }
                }
            }

            return betTypeOddsList;
        }

        private static BetTypeOdds BuildBetTypeOdds((int, string) betType, Book book)
            => new BetTypeOdds
            {
                Id = betType.Item1,
                Name = betType.Item2,
                Bookmaker = new Bookmaker
                {
                    Id = book.id,
                    Name = book.name
                },
                BetOptions = book.outcomes?.Select(oc => BuildBetOption(betType, oc))
            };

        private static BetOptionOdds BuildBetOption((int, string) betType, Outcome oc)
        {
            var betOption = new BetOptionOdds
            {
                Id = oc.odds_field_id,
                LiveOdds = oc.odds == null
                                    ? 0
                                    : Math.Round(decimal.Parse(oc.odds), numOfRounding),

                Type = oc.type,
                OptionValue = GetOptionValue(betType.Item1, oc),
                Name = GetOptionName(betType.Item1, oc.type)
            };

            betOption.OpeningOdds = oc.opening_odds == null
                                    ? betOption.LiveOdds
                                    : Math.Round(decimal.Parse(oc.opening_odds), numOfRounding);

            betOption.OpeningOptionValue = GetOpeningOptionValue(betType.Item1, oc);

            return betOption;
        }

        private static string GetOptionValue(int betTypeId, Outcome oc)
        {
            switch (betTypeId)
            {
                case AsianHandicapBetTypeId:
                    return oc.handicap;

                case OverUnderBeTypeId:
                    return oc.total;

                default:
                    return string.Empty;
            }
        }

        private static string GetOpeningOptionValue(int betTypeId, Outcome oc)
        {
            switch (betTypeId)
            {
                case OverUnderBeTypeId:
                    return oc.opening_total?.ToString();

                default:
                    return string.Empty;
            }
        }

        private static (int, string) GetBetType(Market market)
        {
            switch (market.name)
            {
                case OneXTwoSportRadarBetTypeName:
                    return (OneXTwoBetTypeId, OddsLang.OneXTwo);

                case OverUnderSportRadarBetTypeName:
                    return (OverUnderBeTypeId, OddsLang.OverUnder);

                case AsianHandicapSportRadarBetTypeName:
                    return (AsianHandicapBetTypeId, OddsLang.Handicap);

                default:
                    return (UnknownBetTypeId, UnSupportedBetTypeName);
            }
        }

        private static string GetOptionName(int betTypeId, string type)
        {
            switch (betTypeId)
            {
                case OneXTwoBetTypeId:
                    return type == home
                        ? one
#pragma warning disable S3358 // Ternary operators should not be nested
                        : type == away
                            ? two
                            : x;
#pragma warning restore S3358 // Ternary operators should not be nested

                case OverUnderBeTypeId:
                    return type == over
                        ? OddsLang.Over
                        : OddsLang.Under;

                default:
                    return type;
            }
        }
    }
}