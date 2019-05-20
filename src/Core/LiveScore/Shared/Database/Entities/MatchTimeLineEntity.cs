namespace LiveScore.Shared.Database.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class MatchTimelineEntity
    {
        public long Id { get; set; }

        [Column(TypeName = "jsonb")]
        public string Value { get; set; }

        public string MatchId { get; set; }
    }
}
