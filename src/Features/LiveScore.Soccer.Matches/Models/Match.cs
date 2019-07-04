namespace LiveScore.Soccer.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Soccer.Helpers;
    using LiveScore.Soccer.Models;
    using Newtonsoft.Json;

    public class Match : Entity<string, string>
    {
        public DateTime EventDate { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<IEnumerable<Team>>))]
        public IEnumerable<ITeam> Teams { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<MatchResult>))]
        public MatchResult MatchResult { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<League>))]
        public League League { get; set; }

        public LeagueRound LeagueRound { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<IEnumerable<Timeline>>))]
        public IEnumerable<Timeline> TimeLines { get; set; }

        public int Attendance { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<Venue>))]
        public Venue Venue { get; set; }

        public string Referee { get; set; }

        public string Region { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<Timeline>))]
        public Timeline LatestTimeline { get; set; }

        public IEnumerable<MatchFunction> Functions { get; set; }

        public Match ChangeEventDateByTimeZone(TimeSpan timeZone)
        {
            EventDate = EventDate.ToUniversalTime() + timeZone;

            return this;
        }
    }
}