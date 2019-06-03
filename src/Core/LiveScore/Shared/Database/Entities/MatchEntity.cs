namespace LiveScore.Shared.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MatchEntity : DatabaseEntity
    {
        [Column(TypeName = "jsonb")]
        public string Value { get; set; }

        public DateTime EventDate { get; set; }

        public string LeagueId { get; set; }

        [Key]
        public string Language { get; set; }

        public int SportId { get; set; }
    }
}
