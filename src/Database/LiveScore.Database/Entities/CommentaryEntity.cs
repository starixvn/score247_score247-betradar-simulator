namespace LiveScore.Soccer.Database.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class CommentaryEntity : DatabaseEntity
    {
        [Column(TypeName = "json")]
        public string Value { get; set; }

        [Key]
        public string Language { get; set; }

        public string MatchId { get; set; }
    }
}
