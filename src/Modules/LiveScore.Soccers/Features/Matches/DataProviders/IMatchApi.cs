namespace LiveScore.Soccers.Features.Matches.DataProviders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;

    public interface IMatchApi
    {
        Task<IEnumerable<IMatch>> GetDailySchedules(
            int sportId,
            DateTime from,
            DateTime to,
            string language);

        Task<IEnumerable<IMatch>> GetLiveSchedules(
          int sportId,
          string language);
    }
}