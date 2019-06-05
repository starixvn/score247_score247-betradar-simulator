namespace LiveScore.Features.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models;
    using Newtonsoft.Json;

    public interface IMatch : IEntity<string, string>
    {
        DateTimeOffset EventDate { get; }

        ILeague League { get; }

        LeagueRound LeagueRound { get; }

        IEnumerable<ITeam> Teams { get; }

        IMatchResult MatchResult { get; }

        IEnumerable<ITimeline> TimeLines { get; }

        int Attendance { get; }

        IVenue Venue { get; }

        string Referee { get; }

        IMatch ChangeEventDateByTimeZone(TimeSpan timeZone);
    }

    public class Match : Entity<string, string>, IMatch
    {
        public DateTimeOffset EventDate { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<IEnumerable<Team>>))]
        public IEnumerable<ITeam> Teams { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<MatchResult>))]
        public IMatchResult MatchResult { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<League>))]
        public ILeague League { get; set; }

        public LeagueRound LeagueRound { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<IEnumerable<Timeline>>))]
        public IEnumerable<ITimeline> TimeLines { get; set; }

        public int Attendance { get; set; }

        public IVenue Venue { get; set; }

        public string Referee { get; set; }

        public IMatch ChangeEventDateByTimeZone(TimeSpan timeZone)
        {
            EventDate = EventDate.ToUniversalTime() + timeZone;

            return this;
        }
    }
}