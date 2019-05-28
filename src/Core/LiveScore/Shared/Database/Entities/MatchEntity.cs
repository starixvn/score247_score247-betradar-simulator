namespace LiveScore.Shared.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MatchEntity
    {
        public string Id { get; set; }

        [Column(TypeName = "jsonb")]
        public string Value { get; set; }

        public DateTime EventDate { get; set; }

        public string LeagueId { get; set; }

        public string Language { get; set; }

        public int SportId { get; set; }

        public bool IsLive { get; set; }
    }
}
