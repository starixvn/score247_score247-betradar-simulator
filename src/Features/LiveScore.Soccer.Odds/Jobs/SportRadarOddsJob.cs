namespace LiveScore.Soccer.Odds.Jobs
{
    using System;
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
        private readonly OddsService oddsService;
        private readonly Func<DateTime> getCurrentTimeFunc;
        private readonly IAppSettings appSettings;

        public SportRadarOddsJob(
            IOddsFetcher oddsFetcher,
            OddsService oddsService,
            Func<DateTime> getCurrentTimeFunc,
            IAppSettings appSettings)
        {
            this.oddsFetcher = oddsFetcher;
            this.oddsService = oddsService;
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

                await InsertOdds(oddSchedule);
            }
        }

        private async Task InsertOdds(Providers.SportRadar.Dtos.OddsScheduleDto oddSchedule)
        {
            if (oddSchedule.sport_events != null
                                && oddSchedule.sport_events.Any())
            {
                var oddsMatches = SportRadarOddsBuilder.BuildMatchOddsList(oddSchedule);

                foreach (var oddsMatch in oddsMatches)
                {
                    await oddsService.InsertOdds(oddsMatch);
                }
            }
        }

        public async Task FetchOddsChange()
        {
            var oddSchedule = await oddsFetcher.FetchChangeLog();

            await InsertOdds(oddSchedule);
        }
    }
}