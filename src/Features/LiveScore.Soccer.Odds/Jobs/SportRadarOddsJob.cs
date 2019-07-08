namespace LiveScore.Soccer.Odds.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Odds;
    using LiveScore.Providers.SportRadar.Services;
    using LiveScore.Soccer.Configurations;
    using LiveScore.Soccers.Shared.ModelBuilders;

    public interface ISportRadarOddsJob
    {
        Task FetchDaily();

        Task FetchOddsChange();
    }

    public class SportRadarOddsJob : ISportRadarOddsJob
    {
        private readonly IOddsFetcher oddsFetcher;
        private readonly OddsDataAccess oddsDataAccess;
        private readonly Func<DateTime> getCurrentTimeFunc;
        private readonly IAppSettings appSettings;

        public SportRadarOddsJob(
            IOddsFetcher oddsFetcher, 
            OddsDataAccess oddsDataAccess, 
            Func<DateTime> getCurrentTimeFunc,
            IAppSettings appSettings)
        {
            this.oddsFetcher = oddsFetcher;
            this.oddsDataAccess = oddsDataAccess;
            this.getCurrentTimeFunc = getCurrentTimeFunc;
            this.appSettings = appSettings;
        }

        public async Task FetchDaily()
        {
            var from = getCurrentTimeFunc().ToUniversalTime();
            var to = from.AddDays(appSettings.Jobs.FetchScheduleDateSpan);

            const int dayIncrementIndex = 1;

            for (var date = from.Date; date.Date <= to.Date; date = date.AddDays(dayIncrementIndex))
            {
                var oddSchedule = await oddsFetcher.Fetch(date);
                var oddsMatches = SportRadarOddsBuilder.BuildMatchOddsList(oddSchedule);

                foreach (var oddsMatch in oddsMatches)
                {
                    await oddsDataAccess.InsertOdds(oddsMatch);
                }
            }
        }

        public Task FetchOddsChange()
        {
            throw new NotImplementedException();
        }
    }
}
