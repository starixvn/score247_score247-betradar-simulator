namespace LiveScore.Shared.Database
{
    using LiveScore.Shared.Database.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ScoreAppContext : DbContext
    {
        public ScoreAppContext(DbContextOptions<ScoreAppContext> options)
            : base(options)
        {
        }

        public DbSet<MatchEntity> Matches { get; set; }

        public DbSet<MatchTimelineEntity> Timelines { get; set; }

        public DbSet<MatchResultEntity> MatchResults { get; set; }
    }
}
