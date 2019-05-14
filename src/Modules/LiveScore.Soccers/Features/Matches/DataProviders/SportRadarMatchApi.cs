namespace LiveScore.Soccers.Features.Matches.DataProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models.Dtos;

    public class SportRadarMatchApi : IMatchApi
    {
        private static IDictionary<int, Func<string, DateTime, string, string>> FormatEndPointFuncs = 
            new Dictionary<int, Func<string, DateTime, string, string>>
        {
            { 1,  FormatEndPointType1 },
            { 2,  FormatEndPointType2 }
        };

        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private const string FeatureKey = "Match";

        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarMatchApi(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public async Task<DailyScheduleDto> GetDailySchedules(int sportId, DateTime from, DateTime to, string language)
        {
            var sportSetting = appSettings
                .DataProviderSettings
                .Sports.FirstOrDefault(sport => sport.Id == sportId)
                ?.EndPoints.FirstOrDefault(x => x.Name.Equals(FeatureKey));

            if (sportSetting == null)
            {
                throw new InvalidOperationException("Please config Sport Setting before using");
            }

            var dailyScheduleDtoSum = new DailyScheduleDto { generated_at = DateTime.Now.ToUniversalTime(), results = new List<Result>() };
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();
            const int dayIncrementIndex = 1;

            for (var date = fromUtc.Date; date.Date <= toUtc.Date; date = date.AddDays(dayIncrementIndex))
            {
                foreach (var endPoint in sportSetting.EndPointSettings)
                {
                    var formatEndPointFunc = FormatEndPointFuncs.ContainsKey(endPoint.FormatId)
                        ? FormatEndPointFuncs[endPoint.FormatId]
                        : throw new InvalidOperationException($"Missing function format for endpoint {endPoint.Path}");
                    var formattedEndPoint = formatEndPointFunc(endPoint.Path, date, language);
                    var dailyResult = await restClient.ExecuteGetAsync<DailyScheduleDto>(formattedEndPoint);

                    if (dailyResult.results != null 
                        && dailyResult.results.Any())
                    {
                        var resultInRange = dailyResult.results.Where(result => result.sport_event.scheduled >= from && result.sport_event.scheduled <= to);
                        dailyScheduleDtoSum.results = dailyScheduleDtoSum.results.Concat(resultInRange);
                    }
                }
            }

            return await Task.FromResult(dailyScheduleDtoSum);
        }

        private static string FormatEndPointType1(string endPoint, DateTime date, string language)
            => string.Format(endPoint, language, date.ToString(SportRadarDateFormat));

#pragma warning disable S1172 // Unused method parameters should be removed
        private static string FormatEndPointType2(string endPoint, DateTime date, string language)
#pragma warning restore S1172 // Unused method parameters should be removed
            => string.Format(endPoint, language);
    }
}