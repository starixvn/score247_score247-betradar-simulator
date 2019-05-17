namespace LiveScore.Entity.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MatchEntity
    {
        public string Id { get; set; }

        [Column(TypeName = "jsonb")]
        public string Value { get; set; }

        public DateTime EventDate { get; set; }

        public string SeasonId { get; set; }
    }
}
