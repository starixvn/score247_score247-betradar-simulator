namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Providers.SportRadar.Dtos;

    public class TournamentRound
    {
        public string type { get; set; }
        public int number { get; set; }
        public string group { get; set; }
        public string name { get; set; }
        public int? cup_round_match_number { get; set; }
        public int? cup_round_matches { get; set; }
        public int? tournament_match_number { get; set; }
    }

    public class Sport2
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Outcome
    {
        public int odds_field_id { get; set; }
        public string type { get; set; }
        public string odds { get; set; }
        public string opening_odds { get; set; }
        public string deep_link { get; set; }
        public string odds_trend { get; set; }
        public double? opening_total { get; set; }
        public string total { get; set; }
        public string lay_odds { get; set; }
        public string back_odds { get; set; }
        public string handicap { get; set; }
        public string spread { get; set; }
        public double? opening_spread { get; set; }
    }

    public class Book
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<Outcome> outcomes { get; set; }
    }

    public class Market
    {
        public int odds_type_id { get; set; }
        public string name { get; set; }
        public string group_name { get; set; }
        public List<Book> books { get; set; }
    }

    public class Outcome2
    {
        public string type { get; set; }
        public string odds { get; set; }
        public string total { get; set; }
    }

    public class Line
    {
        public string name { get; set; }
        public List<Outcome2> outcomes { get; set; }
        public string total { get; set; }
        public string spread { get; set; }
    }

    public class Consensus
    {
        public List<Line> lines { get; set; }
    }

    public class SportEvent
    {
        public string id { get; set; }
        public DateTime scheduled { get; set; }
        public bool start_time_tbd { get; set; }
        public string status { get; set; }
        public TournamentRound tournament_round { get; set; }
        public Season season { get; set; }
        public Tournament tournament { get; set; }
        public List<Competitor> competitors { get; set; }
        public List<Market> markets { get; set; }
        public DateTime markets_last_updated { get; set; }
        public Consensus consensus { get; set; }
        public VenueDto venue { get; set; }
        public string uuids { get; set; }
    }

    public class OddsScheduleDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public Sport sport { get; set; }
        public List<SportEvent> sport_events { get; set; }
    }

    public class MatchOddsDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public SportEvent sport_event { get; set; }
    }
}