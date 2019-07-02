namespace LiveScore.Soccer.Shared.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MatchEntity : DatabaseEntity
    {
        [Column(TypeName = "jsonb")]
        public string Value { get; set; }

        public DateTime EventDate { get; set; }

        public string LeagueId { get; set; }

        [Key]
        public string Language { get; set; }

        public int SportId { get; set; }

        public string Region { get; set; }
    }

    public class MatchEntityConfiguration : IEntityTypeConfiguration<MatchEntity>
    {
        public void Configure(EntityTypeBuilder<MatchEntity> builder)
        {
            builder
                .Property(e => e.Value)
                .IsConcurrencyToken();
        }
    }
}