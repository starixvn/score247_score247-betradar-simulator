namespace LiveScore.Features.Sports.Models
{
    using System.Collections.Generic;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Shared.Models;

    public interface ISport : IEntity<string, string>
    {
        IEnumerable<ILeague> Leagues { get; set; }
    }

    public class Sport : Entity<string, string>, ISport
    {
        public IEnumerable<ILeague> Leagues { get; set; }
    }
}