namespace LiveScore.Entity
{
    using LiveScore.Entity.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ScoreAppContext : DbContext
    {
        public ScoreAppContext(DbContextOptions<ScoreAppContext> options)
            : base(options)
        {
        }

        public DbSet<MatchEntity> Matches { get; set; }
    }
}
