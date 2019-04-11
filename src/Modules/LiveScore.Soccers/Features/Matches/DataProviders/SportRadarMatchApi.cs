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
        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarMatchApi(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public async Task<DailyScheduleDto> GetDailySchedules(int sportId, DateTime date, string lang)
        {
            var sportSetting = appSettings.DataProviderSettings.Sports.FirstOrDefault(sport => sport.Id == sportId);
            if (sportSetting == null)
            {
                throw new InvalidOperationException("Please config Sport Setting before using");
            }

            var dailyScheduleDtoSum = new DailyScheduleDto { generated_at = DateTime.Now, results = new List<Result>() };

            foreach (var endPoint in sportSetting.EndPoints)
            {
                var formattedEndPoint = string.Format(endPoint, lang, date.ToString(SportRadarDateFormat));
                var dailyResult = await restClient.ExecuteGetAsync<DailyScheduleDto>(formattedEndPoint);

                dailyScheduleDtoSum.results = dailyScheduleDtoSum.results.Concat(dailyResult.results).ToList();
            }

            return await Task.FromResult(dailyScheduleDtoSum);
        }
    }
}