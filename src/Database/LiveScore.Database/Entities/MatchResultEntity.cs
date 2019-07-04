namespace LiveScore.Soccer.Database.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class MatchResultEntity : DatabaseEntity
    {
        [Column(TypeName = "jsonb")]
        public string Value { get; set; }

        public string MatchId { get; set; }
    }
}
