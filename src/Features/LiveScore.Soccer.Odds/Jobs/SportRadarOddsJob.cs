namespace LiveScore.Soccer.Odds.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface ISportRadarOddsJob
    {
        Task FetchDaily();

        Task FetchOddsChange();
    }

    public class SportRadarOddsJob : ISportRadarOddsJob
    {
        public Task FetchDaily()
        {
            throw new NotImplementedException();
        }

        public Task FetchOddsChange()
        {
            throw new NotImplementedException();
        }
    }
}
