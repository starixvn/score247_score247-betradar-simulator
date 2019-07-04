namespace LiveScore.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using LiveScore.Soccer.Shared.Database.Entities;

    public class OddsEntity : DatabaseEntity
    {
        public new DateTimeOffset CreatedTime { get; set; }

        public new DateTimeOffset ModifiedTime { get; set; }

        [Column(TypeName = "json")]
        public string Value { get; set; }

        public string MatchId { get; set; }

        public int BetTypeId { get; set; }

        public string BookmakerId { get; set; }
    }
}