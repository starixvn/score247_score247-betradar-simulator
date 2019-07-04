namespace LiveScore.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using LiveScore.Soccer.Database.Entities;

    public class OddsEntity : DatabaseEntity
    {
        [Column(TypeName = "json")]
        public string Value { get; set; }

        public string MatchId { get; set; }

        public int BetTypeId { get; set; }

        public string BookmakerId { get; set; }
    }
}