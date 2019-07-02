using System;
using LiveScore.Soccer.Shared.Database.Entities;

namespace LiveScore.Soccers.UnitTests.Shared
{
    public static class StubMatch
    {

        public static MatchEntity StubMatchEntity()
        {
            return new MatchEntity
            {
                Id = "sr:match:17809194",
                Value = StubMatchJson(),
                LeagueId = "sr:tournament:28424",
                Language = "en-US",
                SportId = 1,
                EventDate = DateTime.Parse("2019-05-22 00:00:00+00")
            };
        }

        public static string StubMatchJson()
        {
            return @"{
                    'Id': 'sr:match:17809194',
                    'Name': null,
                    'Teams': [
                        {
                            'Id': 'sr:competitor:559146',
                            'Flag': null,
                            'Name': 'Denton Diablos FC',
                            'Coach': null,
                            'IsHome': true,
                            'Country': 'USA',
                            'Players': null,
                            'Formation': null,
                            'Statistic': null,
                            'CountryCode': 'USA',
                            'Abbreviation': 'DEN',
                            'Substitutions': null
                        },
                        {
                            'Id': 'sr:competitor:559156',
                            'Flag': null,
                            'Name': 'Dallas City FC',
                            'Coach': null,
                            'IsHome': false,
                            'Country': 'USA',
                            'Players': null,
                            'Formation': null,
                            'Statistic': null,
                            'CountryCode': 'USA',
                            'Abbreviation': 'DAL',
                            'Substitutions': null
                        }
                    ],
                    'Venue': null,
                    'League': {
                        'Id': 'sr:tournament:28424',
                        'Flag': null,
                        'Name': 'National Premier Soccer League',
                        'Order': 0,
                        'Category': {
                            'Id': 'sr:category:26',
                            'Name': 'USA',
                            'CountryCode': 'USA'
                        }
                    },
                    'Referee': null,
                    'EventDate': '2019-05-22T00:00:00Z',
                    'TimeLines': null,
                    'Attendance': 0,
                    'LeagueRound': {
                        'Name': null,
                        'Type': {
                            'Value': 'group',
                            'DisplayName': 'Group'
                        },
                        'Group': null,
                        'Phase': 'regular season',
                        'Number': 1
                    },
                    'MatchResult': {
                        'Period': 0,
                        'WinnerId': null,
                        'AwayScore': 0,
                        'HomeScore': 0,
                        'MatchTime': 0,
                        'EventStatus': {
                            'Value': 'not_started',
                            'DisplayName': 'NotStarted'
                        },
                        'MatchStatus': {
                            'Value': 'not_started',
                            'DisplayName': 'NotStarted'
                        },
                        'MatchPeriods': null
                    }
                }";
        }

        public static MatchEntity StubMatchEntityWithLiveResult()
        {
            return new MatchEntity
            {
                Id = "sr:match:17809194",
                Value = StubMatchWithLiveResultJson(),
                LeagueId = "sr:tournament:28424",
                Language = "en-US",
                SportId = 1,
                EventDate = DateTime.Parse("2019-05-22 00:00:00+00")
            };
        }

        public static LiveMatchEntity StubLiveMatchEntity()
        {
            return new LiveMatchEntity
            {
                Id = "sr:match:17809194",
                Value = StubMatchWithLiveResultJson(),
                LeagueId = "sr:tournament:28424",
                Language = "en-US",
                SportId = 1,
                EventDate = DateTime.Parse("2019-05-22 00:00:00+00")
            };
        }

        public static string StubMatchWithLiveResultJson()
        {
            return @"{
                    'Id': 'sr:match:17809194',
                    'Name': null,
                    'Teams': [
                        {
                            'Id': 'sr:competitor:559146',
                            'Flag': null,
                            'Name': 'Denton Diablos FC',
                            'Coach': null,
                            'IsHome': true,
                            'Country': 'USA',
                            'Players': null,
                            'Formation': null,
                            'Statistic': null,
                            'CountryCode': 'USA',
                            'Abbreviation': 'DEN',
                            'Substitutions': null
                        },
                        {
                            'Id': 'sr:competitor:559156',
                            'Flag': null,
                            'Name': 'Dallas City FC',
                            'Coach': null,
                            'IsHome': false,
                            'Country': 'USA',
                            'Players': null,
                            'Formation': null,
                            'Statistic': null,
                            'CountryCode': 'USA',
                            'Abbreviation': 'DAL',
                            'Substitutions': null
                        }
                    ],
                    'Venue': null,
                    'League': {
                        'Id': 'sr:tournament:28424',
                        'Flag': null,
                        'Name': 'National Premier Soccer League',
                        'Order': 0,
                        'Category': {
                            'Id': 'sr:category:26',
                            'Name': 'USA',
                            'CountryCode': 'USA'
                        }
                    },
                    'Referee': null,
                    'EventDate': '2019-05-22T00:00:00Z',
                    'TimeLines': null,
                    'Attendance': 0,
                    'LeagueRound': {
                        'Name': null,
                        'Type': {
                            'Value': 'group',
                            'DisplayName': 'Group'
                        },
                        'Group': null,
                        'Phase': 'regular season',
                        'Number': 1
                    },
                    'MatchResult': {
                        'Period': 0,
                        'WinnerId': null,
                        'AwayScore': 0,
                        'HomeScore': 1,
                        'MatchTime': 22,
                        'EventStatus': {
                            'Value': 'live',
                            'DisplayName': 'Live'
                        },
                        'MatchStatus': {
                            'Value': '1st_half',
                            'DisplayName': 'Halftime'
                        },
                        'MatchPeriods': null
                    },
                    'Timelines':[{
                        'Id': 555147301,
                        'Name': null,
                        'Team': null,
                        'Time': '2019-05-09T02:47:17',
                        'Type': 'injury_time_shown',
                        'Assist': null,
                        'Period': 1,
                        'Outcome': null,
                        'PlayerIn': null,
                        'AwayScore': 0,
                        'HomeScore': 0,
                        'MatchTime': 45,
                        'PlayerOut': null,
                        'GoalScorer': null,
                        'MatchClock': '45:00',
                        'PeriodType': null,
                        'Description': null,
                        'Commentaries': null,
                        'InjuryTimeAnnounced': 2
                    }]
                }";
        }
    }
}
